using System;
using NLog;
using System.Configuration;
using NLog.Targets;

namespace NativeClient
{
    class MainClass
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            Logger.Debug("start main logger");
            Console.WriteLine("start main console");

            ConfigureNLog(args);

            int clientId = GetClientId(args);
            var clientDriver = new ClientDriver(clientId);

            Logger.Info("Reading configuration");

            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            clientDriver.Configure(ConfigurationManager.AppSettings);

            Logger.Info("Starting simulation");

            clientDriver.StartSimulation();

            // Wait for 'q' key to be pressed.
            Console.WriteLine("The client is up and running. Press Enter to stop it...");
            Console.In.ReadLine();
            Environment.Exit(0);
        }

        static int GetClientId(string[] args)
        {
            int clientId = -1;
            foreach (string arg in args)
            {
                if (arg.StartsWith("--clientId="))
                    Int32.TryParse(arg.Substring(11), out clientId);
            }

            return clientId;
        }

        /// <summary>
        /// Configures NLog based on command line arguments.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        static void ConfigureNLog(string[] args)
        {
            string loggingFile = null;
            foreach (string arg in args)
            {
                if (arg.StartsWith("--logfile="))
                    loggingFile = arg.Substring(10);
            }

            if (loggingFile != null)
            {
                var fileTarget = LogManager.Configuration.FindTargetByName("logfile") as FileTarget;
                if (fileTarget != null)
                    fileTarget.FileName = loggingFile;
            }
            else
            {
                LogManager.Configuration.RemoveTarget("logfile");
            }
        }
    }
}
