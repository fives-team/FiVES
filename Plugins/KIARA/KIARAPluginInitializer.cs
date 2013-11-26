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
            var protocolDir = GetProtocolDir();
            LoadProtocols(protocolDir);
        }

        #endregion

        static void LoadProtocols(string protocolDir)
        {
            logger.Info("Loading protocols");
        
            if (protocolDir != null && Directory.Exists(protocolDir))
            {
                ProtocolRegistry.Instance.LoadProtocolsFrom(protocolDir);
            }
            else
            {
                logger.Error("Protocol dir is not specified or does not exist");
            }

            logger.Info("Finished loading protocols");
        }

        static string GetProtocolDir()
        {
            string protocolDir = null;
            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            try
            {
                protocolDir = ConfigurationManager.AppSettings["ProtocolDir"].ToString();
            }
            catch (ConfigurationErrorsException cee)
            {
                logger.Error("KIARA configuration is missing or corrupt.");
                throw cee;
            }

            return protocolDir;
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}