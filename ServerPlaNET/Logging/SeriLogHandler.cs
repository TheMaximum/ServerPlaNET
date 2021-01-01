using Serilog;
using Serilog.Core;
using ServerPlaNET.Interfaces;
using System;
using System.Diagnostics;
using System.Reflection;

namespace ServerPlaNET.Logging
{
    public class SeriLogHandler : ILogHandler
    {
        private Logger logger;

        public SeriLogHandler()
        {
            logger = new LoggerConfiguration()
                .Enrich.With(new ThreadIdEnricher())
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level,-11:t11}] {Message:lj}{NewLine}{Exception}",
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
                .WriteTo.File("logs\\serverplanet.log", 
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} {ThreadId,-9} [{Level,-11:t11}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            LogInit();
        }

        private void LogInit()
        {
            string processName = Process.GetCurrentProcess().ProcessName;
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string userName = Environment.UserName;
            OperatingSystem operatingSystem = Environment.OSVersion;

            string message = $"{processName} [{version}] on {operatingSystem.Platform} ({operatingSystem.VersionString}) [user: {userName}]";

            logger.Debug(" -> {ThreadId} " + message);
        }

        public void Verbose(string message, params object[] parameters)
        {
            logger.Verbose(message, parameters);
        }

        public void Debug(string message, params object[] parameters)
        {
            logger.Debug(message, parameters);
        }

        public void Information(string message, params object[] parameters)
        {
            logger.Information(message, parameters);
        }

        public void Warning(string message, params object[] parameters)
        {
            logger.Warning(message, parameters);
        }

        public void Error(Exception exception, string message, params object[] parameters)
        {
            logger.Error(exception, message, parameters);
        }

        public void Error(string message, params object[] parameters)
        {
            logger.Error(message, parameters);
        }

        public void Fatal(Exception exception, string message, params object[] parameters)
        {
            logger.Fatal(exception, message, parameters);
        }

        public void Fatal(string message, params object[] parameters)
        {
            logger.Fatal(message, parameters);
        }
    }
}
