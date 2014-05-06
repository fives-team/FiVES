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