// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FIVES;
using NLog;

namespace StaticSceneryPlugin
{
    /// <summary>
    /// Adds an Entity as static scenery to the world on startup. The URL to the scenery file as well as the y-offset to define the height
    /// of the ground plane are provided in a config file
    /// </summary>
    public class StaticSceneryPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get { return "StaticScenery"; }
        }

        public List<string> PluginDependencies
        {
            get { return new List<string> (); }
        }

        public List<string> ComponentDependencies
        {
            get { return new List<string> { "location", "mesh" }; }
        }

        public void Initialize()
        {
            try
            {
                ReadConfig();
            }
            catch (Exception)
            {
                logger.Warn("Could not read Config for StaticScenery. Using default values instead.");
            }
            finally
            {
                CreateSceneryEntity();
            }
        }

        public void Shutdown()
        {
        }

        /// <summary>
        /// Reads scenery-model-uri and offsets from app-config
        /// </summary>
        private void ReadConfig()
        {
            string sceneryConfigPath = this.GetType().Assembly.Location;
            Configuration config = ConfigurationManager.OpenExeConfiguration(sceneryConfigPath);

            SceneryURL = config.AppSettings.Settings["sceneryUrl"].Value;

            string offsetXConfigValue = config.AppSettings.Settings["offsetX"].Value;
            string offsetYConfigValue = config.AppSettings.Settings["offsetY"].Value;
            string offsetZConfigValue = config.AppSettings.Settings["offsetZ"].Value;

            float.TryParse(offsetXConfigValue, out OffsetX);
            float.TryParse(offsetYConfigValue, out OffsetY);
            float.TryParse(offsetZConfigValue, out OffsetZ);
        }

        /// <summary>
        /// Creates the scenery entity and adds it to the world
        /// </summary>
        internal void CreateSceneryEntity()
        {
            Entity sceneryEntity = new Entity();
            sceneryEntity["mesh"]["uri"].Suggest(SceneryURL);
            sceneryEntity["location"]["position"].Suggest(new Vector(OffsetX, OffsetY, OffsetZ));
            World.Instance.Add(sceneryEntity);
        }

        internal string SceneryURL = "";
        internal float OffsetX = 0f;
        internal float OffsetY = 0f;
        internal float OffsetZ = 0f;

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}
