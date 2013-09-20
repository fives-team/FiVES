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
            if (pluginDir != null && Directory.Exists(pluginDir))
                PluginManager.Instance.LoadPluginsFrom(pluginDir);
            else
                logger.Error("Plugin dir is not specified or does not exist");

            logger.Info("Loading complete");

//            // Add 5 entities.
//            var random = new Random();
//            for (int i = 0; i < 5; i++) {
//                var entity = new Entity();
//                entity["position"]["x"] = (float)(random.NextDouble() * 100 - 50);
//                entity["position"]["y"] = (float)(random.NextDouble() * 100 - 50);
//                entity["position"]["z"] = (float)(random.NextDouble() * 100 - 50);
//
//                entity["scale"]["x"] = 1.0f;
//                entity["scale"]["y"] = 1.0f;
//                entity["scale"]["z"] = 1.0f;
//
//                entity["meshResource"]["uri"] = "my://path.to/resource";
//
//                EntityRegistry.Instance.addEntity(entity);
//            }

            Console.WriteLine("The server is up and running. Press any key to stop it...");

            // Wait for any key to be pressed.
            Console.ReadKey();

            return 0;
        }
    }
}

