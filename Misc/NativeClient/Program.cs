// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
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
            Console.WriteLine("The client is up and running. Press Enter to stop it...");
            Console.In.ReadLine();
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
