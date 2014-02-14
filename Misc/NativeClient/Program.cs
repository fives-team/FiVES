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
            ConfigureNLog(args);

            var clientDriver = new ClientDriver();

            Logger.Info("Reading configuration");

            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            clientDriver.Configure(ConfigurationManager.AppSettings);

            Logger.Info("Starting simulation");

            clientDriver.StartSimulation();

            // Wait for 'q' key to be pressed.
            Console.WriteLine("The server is up and running. Press 'q' to stop it...");
            while (Console.ReadKey().KeyChar != 'q');
            Environment.Exit(0);
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
