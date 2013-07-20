using System;
using System.Configuration;
using NLog;
using System.IO;

namespace FIVES
{
    public class MainClass
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static int Main(string[] args)
        {
            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            try {
                string pluginDir = ConfigurationManager.AppSettings["PluginsDir"].ToString();
                if (Directory.Exists(pluginDir))
                    PluginManager.Instance.loadPluginsFrom(pluginDir);
            } catch (NullReferenceException) {
                logger.Error("Plugins dir is not specified.");
            } catch (ConfigurationErrorsException) {
                logger.Error("Configuration is missing or corrupt.");
            }

            // Add 20 entities.
            for (int i = 1; i < 20; i++)
                EntityRegistry.addEntity(new Entity());

            // Wait for any key to be pressed.
            Console.ReadKey();

            return 0;
        }
    }
}

