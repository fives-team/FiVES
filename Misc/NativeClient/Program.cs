using System;
using NLog;
using System.Configuration;

namespace NativeClient
{
    class MainClass
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            var clientDriver = new ClientDriver();

            Logger.Info("Reading configuration");

            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            clientDriver.Configure(ConfigurationManager.AppSettings);

            Logger.Info("Starting simulation");

            clientDriver.StartSimulation();

            Console.WriteLine("Client is running. Please any key to quit...");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
