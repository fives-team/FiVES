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
            Logger.Info("Reading configuration");

            string host = null;
            int ip = 0;
            string uri = null;
            bool enableMovement, enableRotation;

            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            try {
                uri = ConfigurationManager.AppSettings["ServerURI"];
                host = ConfigurationManager.AppSettings["ServerHost"];
                Int32.TryParse(ConfigurationManager.AppSettings["ServerPort"], out ip);
                enableMovement = ConfigurationManager.AppSettings["EnableMovement"] == "true";
                enableRotation = ConfigurationManager.AppSettings["EnableRotatation"] == "true";
            } catch (ConfigurationErrorsException e) {
                Logger.FatalException("Configuration is missing or corrupt.", e);
                return;
            }

            Logger.Info("Initiailizing client");

            new ClientDriver(uri, host, ip, enableMovement, enableRotation);

            Console.WriteLine("Client is running. Please any key to quit...");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
