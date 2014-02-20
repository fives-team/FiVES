using System;
using FIVES;
using System.Collections.Generic;
using System.Configuration;
using NLog;
using System.IO;

namespace KIARAPlugin
{
    public class KIARAPluginInitializer : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string Name
        {
            get
            {
                return "KIARA";
            }
        }

        public List<string> PluginDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        public List<string> ComponentDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        public void Initialize()
        {
            string protocolDir;
            string[] pluginWhiteList;
            LoadConfig(out protocolDir, out pluginWhiteList);
            LoadProtocols(protocolDir, pluginWhiteList);
        }

        public void Shutdown()
        {
        }

        #endregion

        static void LoadProtocols(string protocolDir, string[] pluginWhiteList)
        {
            logger.Info("Loading protocols");

            if (protocolDir != null && Directory.Exists(protocolDir))
            {
                ProtocolRegistry.Instance.LoadProtocolsFrom(protocolDir, pluginWhiteList);
            }
            else
            {
                logger.Error("Protocol dir is not specified or does not exist");
            }

            logger.Info("Finished loading protocols");
        }

        static void LoadConfig(out string protocolDir, out string[] protocolWhiteList)
        {
            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            try
            {
                protocolDir = ConfigurationManager.AppSettings["ProtocolDir"].ToString();
                string protocolWhiteListStr = ConfigurationManager.AppSettings["ProtocolWhiteList"];
                if (protocolWhiteListStr != null)
                    protocolWhiteList = protocolWhiteListStr.Split(',');
                else
                    protocolWhiteList = null;
            }
            catch (ConfigurationErrorsException cee)
            {
                logger.Error("KIARA configuration is missing or corrupt.");
                throw cee;
            }
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}