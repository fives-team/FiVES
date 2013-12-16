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
            bool enableMovement, enableRotation;

            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            try {
                host = ConfigurationManager.AppSettings["ServerHost"].ToString();
                ip = Int32.Parse(ConfigurationManager.AppSettings["ServerPort"]);
                enableMovement = ConfigurationManager.AppSettings["EnableMovement"].ToString() == "true";
                enableRotation = ConfigurationManager.AppSettings["EnableRotatation"].ToString() == "true";
            } catch (ConfigurationErrorsException e) {
                Logger.FatalException("Configuration is missing or corrupt.", e);
                return;
            }

            Logger.Info("Initiailizing client");

            new ClientDriver(host, ip, enableMovement, enableRotation);

            Console.WriteLine("Client is running. Please any key to quit...");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
