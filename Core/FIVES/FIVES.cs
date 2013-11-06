using System;
using System.Configuration;
using NLog;
using System.IO;
using System.Text;

namespace FIVES
{
    public class MainClass
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static int Main(string[] args)
        {
            // Load configuration options.
            string pluginDir = null;
            string protocolDir = null;
            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            try {
                pluginDir = ConfigurationManager.AppSettings["PluginDir"].ToString();
                protocolDir = ConfigurationManager.AppSettings["ProtocolDir"].ToString();
            } catch (ConfigurationErrorsException) {
                logger.Error("Configuration is missing or corrupt.");
            }

            logger.Info("Loading protocols");
            if (protocolDir != null && Directory.Exists(protocolDir))
                KIARA.ProtocolRegistry.Instance.LoadProtocolsFrom(protocolDir);
            else
                logger.Error("Protocol dir is not specified or does not exist");

            logger.Info("Loading plugins");
            if (pluginDir != null && Directory.Exists(pluginDir)) {
                PluginManager.Instance.LoadPluginsFrom(pluginDir);
                if (PluginManager.Instance.DeferredPlugins.Count > 0) {
                    StringBuilder logEntry = CreateDeferredPluginsLogEntry();
                    logger.Warn(logEntry);
                }
            } else {
                logger.Error("Plugin dir is not specified or does not exist");
            }

            logger.Info("Loading complete");

            Console.WriteLine("The server is up and running. Press any key to stop it...");

            // Wait for any key to be pressed.
            Console.ReadKey();

            return 0;
        }

        private static StringBuilder CreateDeferredPluginsLogEntry()
        {
            StringBuilder logEntry = new StringBuilder();
            logEntry.Append("Failed to load the following plugins due to missing dependencies:\n");
            foreach (var deferredPlugin in PluginManager.Instance.DeferredPlugins)
            {
                logEntry.AppendFormat("{0}: (path: {1}, deps: {2})\n", deferredPlugin.Key,
                    deferredPlugin.Value.path, String.Join(", ", deferredPlugin.Value.remainingDeps));
            }
            return logEntry;
        }
    }
}

