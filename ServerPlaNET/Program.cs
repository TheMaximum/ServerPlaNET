using Microsoft.Extensions.Configuration;
using ServerPlaNET.Logging;
using ServerPlaNET.Remote;
using ServerPlaNET.Remote.Structs;
using System;
using System.Threading.Tasks;

namespace ServerPla.NET
{
    public class Program
    {
        public static GbxRemote remote;

        public static async Task Main(string[] args)
        {
            Console.CancelKeyPress += OnSigInt;
            AppDomain.CurrentDomain.ProcessExit += OnSigTerm;

            SeriLogHandler logger = new SeriLogHandler();

            IConfigurationRoot builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            ServerConfiguration configuration = builder.Get<ServerConfiguration>();

            remote = new GbxRemote(logger, configuration);
            await remote.RunAsync();
        }

        public static async void OnSigInt(object sender, ConsoleCancelEventArgs e)
        {
            Environment.Exit(0);
        }

        public static async void OnSigTerm(object sender, EventArgs e)
        {
            await remote.CloseAsync();
        }
    }
}
