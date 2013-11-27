using System;
using System.Configuration;
using NLog;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace FIVES
{
    public class Application
    {
        public static ApplicationController Controller = null;

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

            // Load configuration options.
            string[] pluginFilters = null;
            try {
                string pluginFilterList = ConfigurationManager.AppSettings["PluginFilters"].ToString();
                pluginFilters = pluginFilterList.Split(',');
            } catch (ConfigurationErrorsException) {
                // This is optional element.
            }

            logger.Info("Loading plugins");
            if (pluginDir != null && Directory.Exists(pluginDir)) {
                PluginManager.Instance.LoadPluginsFrom(pluginDir, pluginFilters);
                if (PluginManager.Instance.DeferredPlugins.Count > 0)
                    logger.Info(CreateDeferredPluginsLogEntry());
            } else {
                logger.Error("Plugin dir is not specified or does not exist");
            }

            var loadedPlugins = new List<PluginManager.PluginInfo>(PluginManager.Instance.LoadedPlugins);
            List<string> loadedPluginNames = loadedPlugins.ConvertAll(info => info.Name);
            logger.Info("Initialized plugins: " + String.Join(", ", loadedPluginNames));

            PluginManager.Instance.OnAnyPluginInitialized += HandlePluginInitialized;

            if (Controller == null)
            {
                // Wait for 'q' key to be pressed.
                Console.WriteLine("The server is up and running. Press 'q' to stop it...");
                while (Console.ReadKey().KeyChar != 'q') ;
            }
            else
            {
                Controller.WaitForTerminate();
            }

            return 0;
        }

        private static void HandlePluginInitialized(object sender, PluginInitializedEventArgs e)
        {
            logger.Info("Plugin " + e.pluginName + " loaded");
        }

        private static string CreateDeferredPluginsLogEntry()
        {
            StringBuilder logEntry = new StringBuilder();
            logEntry.Append("Initalization of the following plugins was deferred due to missing dependencies:\n");
            foreach (PluginManager.PluginInfo pluginInfo in PluginManager.Instance.DeferredPlugins)
            {
                logEntry.AppendFormat("  {0}: (path: {1}, plugin deps: [{2}], component deps: [{3}])\n", pluginInfo.Name,
                                      pluginInfo.Path, String.Join(", ", pluginInfo.RemainingPluginDeps),
                                      String.Join(", ", pluginInfo.RemainingComponentDeps));
            }
            var finalString = logEntry.ToString();
            return finalString.Substring(0, finalString.Length - 1);  // remove trailing "\n"
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}

