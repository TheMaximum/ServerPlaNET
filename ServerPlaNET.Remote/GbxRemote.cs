using ServerPlaNET.Interfaces;
using ServerPlaNET.Remote.Enums;
using ServerPlaNET.Remote.Structs;
using ServerPlaNET.Remote.Structs.Call;
using ServerPlaNET.Remote.Structs.Responses;
using ServerPlaNET.Remote.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ServerPlaNET.Remote
{
    public class GbxRemote
    {
        private const uint serverCallbackMask = 0x80000000;
        private uint requestHandle = serverCallbackMask;

        private ConcurrentDictionary<uint, ManualResetEvent> manualResetEvents;
        private ConcurrentDictionary<uint, MethodResponse> methodResponses;

        private ILogHandler log;
        private TcpClient server;
        private ServerConfiguration configuration;

        private CancellationTokenSource cancellationTokenSource;

        public GbxRemote(ILogHandler logHandler, ServerConfiguration serverConfiguration)
        {
            server = new TcpClient();
            log = logHandler;
            configuration = serverConfiguration;
            cancellationTokenSource = new CancellationTokenSource();

            manualResetEvents = new ConcurrentDictionary<uint, ManualResetEvent>();
            methodResponses = new ConcurrentDictionary<uint, MethodResponse>();
        }

        public async Task RunAsync()
        {
            if (await ConnectAsync())
            {
                Task loop = Task.Run(async () => await LoopAsync(cancellationTokenSource.Token));

                if (await AuthenticateAsync())
                {
                    await CallAsync(MethodName.SetApiVersion, configuration.ApiVersion);
                    await CallAsync(MethodName.EnableCallbacks, configuration.EnableCallbacks);
                }
                else
                {
                    cancellationTokenSource.Cancel();
                }

                Task.WaitAll(loop);

                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    log.Information("Disconnected from the server.");
                    server.Dispose();
                }
            }
        }

        public async Task CloseAsync()
        {
            cancellationTokenSource.Cancel();
            log.Information("Disconnected from the server.");
        }

        public async Task<MethodResponse> CallAsync(string methodName, params object[] parameters)
        {
            MethodCall call = new MethodCall(methodName, parameters);
            string data = string.Empty;

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");

            XmlSerializer serializer = new XmlSerializer(typeof(MethodCall));
            using (UTF8StringWriter writer = new UTF8StringWriter())
            {
                serializer.Serialize(writer, call, namespaces);
                data = writer.ToString();
            }

            // Write data to the server.
            uint requestHandle = await WriteAsync(data);
            log.Debug($"Sent call with method '{methodName}' (request handle: {requestHandle}).");

            // Reset manual event if still exists.
            if (manualResetEvents.ContainsKey(requestHandle))
            {
                manualResetEvents[requestHandle].Set();
                manualResetEvents.Remove(requestHandle, out _);
            }

            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            manualResetEvents[requestHandle] = manualResetEvent;
            manualResetEvents[requestHandle].WaitOne();

            MethodResponse response = methodResponses[requestHandle];

            // Dispose the reset event.
            manualResetEvent.Dispose();

            if (response.Fault.HasValue)
            {
                throw new MethodFaultedException(response);
            }

            return response;
        }

        private async Task<bool> ConnectAsync()
        {
            log.Information("Connecting to server on '{0}:{1}'.", configuration.Address, configuration.Port);

            // Connect to the server using TCP with the provided address and port.
            await server.ConnectAsync(configuration.Address, configuration.Port);

            if (server.Connected)
            {
                ProtocolLength? protocolLength = await ReceiveStructAsync<ProtocolLength>(4);

                if (!protocolLength.HasValue || protocolLength.Value.Length > 64)
                {
                    // Incorrect lowlevel protocol header received.
                    return false;
                }

                string protocol = await ReceiveAsync<string>(protocolLength.Value.Length);

                if (protocol.Contains("GBXRemote 1"))
                {
                    // Old version of TrackMania server detected.
                    log.Error($"Old protocol version '{protocol}' detected, this Remote only works with GBXRemote 2!");
                    return false;
                }
                else if (protocol.Contains("GBXRemote 2"))
                {
                    log.Debug("Received correct protocol version: '{0}'.", protocol);
                }
                else
                {
                    // Incorrect lowlevel protocol version received.
                    log.Error($"Incorrect lowlevel protocol version received: '{protocol}'.");
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> AuthenticateAsync()
        {
            try
            {
                MethodResponse authenticateResponse =
                    await CallAsync(MethodName.Authenticate, configuration.UserName, configuration.Password);

                if (authenticateResponse.Parameters == null || authenticateResponse.Parameters.Length != 1 ||
                    !authenticateResponse.Parameters[0].ParameterValue.Boolean.HasValue || authenticateResponse.Parameters[0].ParameterValue.Boolean.Value == false)
                {
                    log.Error("Unexpected result during authentication.");
                    return false;
                }
            }
            catch (MethodFaultedException ex)
            {
                log.Error("Unable to authenticate with provided credentials (user: '{UserName}'): {Message}", configuration.UserName, ex.Message);
                return false;
            }

            log.Information("Successfully authenticated as '{UserName}'.", configuration.UserName);
            return true;
        }

        private async Task LoopAsync(CancellationToken token)
        {
            while (server != null && server.Connected)
            {
                if (token.IsCancellationRequested)
                {
                    server.Close();
                    break;
                }

                QueryResponse? data = await ReceiveStructAsync<QueryResponse>(8, waitForData: false);
                if (!data.HasValue)
                {
                    continue;
                }

                if (manualResetEvents.ContainsKey(data.Value.Handle))
                {
                    log.Debug($"Received response for request handle: {requestHandle}.");

                    // Handle method response.
                    // Reset manual event if still exists.
                    MethodResponse? response = await ReceiveStructAsync<MethodResponse>(data.Value.Length);
                    if (response.HasValue)
                    {
                        methodResponses.TryAdd(data.Value.Handle, response.Value);
                    }

                    manualResetEvents[data.Value.Handle].Set();
                    manualResetEvents.Remove(data.Value.Handle, out _);
                }
                else
                {
                    byte[] received = await ReceiveAsync(data.Value.Length);
                    using (MemoryStream stream = new MemoryStream(received))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(MethodCall));
                        MethodCall call = (MethodCall)serializer.Deserialize(stream);

                        log.Debug($"Callback received: '{call.MethodName}'");
                    }
                }
            }
        }

        private async Task<uint> WriteAsync(string data)
        {
            // Increase request handle.
            uint handle = GetNextRequestHandle();

            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(((uint)data.Length)));
            bytes.AddRange(BitConverter.GetBytes(handle));
            bytes.AddRange(Encoding.UTF8.GetBytes(data));

            log.Verbose($"-> ({bytes.Count}) {BitConverter.ToString(bytes.ToArray())}");

            NetworkStream stream = server.GetStream();
            await stream.WriteAsync(bytes.ToArray());
            await stream.FlushAsync();

            return handle;
        }

        private async Task<T?> ReceiveAsync<T>(uint length)
        {
            byte[] bytes = await ReceiveAsync(length);
            dynamic result = null;

            if (typeof(T) == typeof(string))
            {
                result = Encoding.UTF8.GetString(bytes);
            }

            return (T)result;
        }

        private async Task<T?> ReceiveStructAsync<T>(uint length, bool waitForData = true)
            where T : struct
        {
            if (!waitForData && server.Available < length)
            {
                return null;
            }

            byte[] bytes = await ReceiveAsync(length);

            // Check if the byte array is an XML string.
            if (bytes[0] == 0x3C)
            {
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    return (T)serializer.Deserialize(stream);
                }
            }
            else
            {
                return StructSerializer.Deserialize<T>(bytes);
            }
        }

        private async Task<byte[]> ReceiveAsync(uint length)
        {
            byte[] buffer = new byte[length];
            int offset = 0;

            NetworkStream stream = server.GetStream();

            while ((length - offset) > 0)
            {
                offset += await stream.ReadAsync(buffer, offset, ((int)length - offset));
            }

            log.Verbose($"<- ({buffer.Length}) {BitConverter.ToString(buffer)}");
            return buffer;
        }

        private uint GetNextRequestHandle()
        {
            if (requestHandle == 0xFFFFFFFF)
            {
                log.Debug("Reached maximum handler number, resetting.");
                requestHandle = 0x80000000;
            }
            else
            {
                requestHandle++;
            }

            return requestHandle;
        }
    }
}
