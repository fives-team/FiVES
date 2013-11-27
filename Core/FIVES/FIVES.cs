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
            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            try {
                pluginDir = ConfigurationManager.AppSettings["PluginDir"].ToString();
            } catch (ConfigurationErrorsException) {
                logger.Error("Configuration is missing or corrupt.");
            }

            logger.Info("Loading plugins");
            if (pluginDir != null && Directory.Exists(pluginDir)) {
                PluginManager.Instance.LoadPluginsFrom(pluginDir);
                if (PluginManager.Instance.DeferredPlugins.Count > 0) {
                    StringBuilder logEntry = CreateDeferredPluginsLogEntry();
                    logger.Info(logEntry);
                }
            } else {
                logger.Error("Plugin dir is not specified or does not exist");
            }

            logger.Info("Loading complete");

            // Wait for 'q' key to be pressed.
            Console.WriteLine("The server is up and running. Press 'q' to stop it...");
            while (Console.ReadKey().KeyChar != 'q');

            return 0;
        }

        private static StringBuilder CreateDeferredPluginsLogEntry()
        {
            StringBuilder logEntry = new StringBuilder();
            logEntry.Append("Loading of the following plugins was deferred due to missing dependencies:\n");
            foreach (var deferredPlugin in PluginManager.Instance.DeferredPlugins)
            {
                logEntry.AppendFormat("{0}: (path: {1}, plugin deps: {2}, component deps: {3})\n", deferredPlugin.Key,
                    deferredPlugin.Value.path, String.Join(", ", deferredPlugin.Value.remainingPluginDeps),
                    String.Join(", ", deferredPlugin.Value.remainingComponentDeps));
            }
            return logEntry;
        }
    }
}

