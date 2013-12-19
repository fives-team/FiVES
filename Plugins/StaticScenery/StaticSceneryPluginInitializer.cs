using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FIVES;

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
            get { return new List<string> {"Renderable", "Location"}; }
        }

        public List<string> ComponentDependencies
        {
            get { return new List<string>(); }
        }

        public void Initialize()
        {
            ReadConfig();
            CreateSceneryEntity();
        }

        /// <summary>
        /// Reads scenery-model-uri and y-offset from app-config
        /// </summary>
        private void ReadConfig()
        {
            string sceneryConfigPath = this.GetType().Assembly.Location;
            Configuration config = ConfigurationManager.OpenExeConfiguration(sceneryConfigPath);

            SceneryURL = config.AppSettings.Settings["sceneryUrl"].Value;
            string offsetYConfigValue = config.AppSettings.Settings["offsetY"].Value;
            float.TryParse(offsetYConfigValue, out OffsetY);
        }

        /// <summary>
        /// Creates the scenery entity and adds it to the world
        /// </summary>
        private void CreateSceneryEntity()
        {
            Entity sceneryEntity = new Entity();
            sceneryEntity["meshResource"]["uri"] = SceneryURL;
            sceneryEntity["position"]["y"] = OffsetY;
            World.Instance.Add(sceneryEntity);
        }

        private string SceneryURL = "";
        private float OffsetY = 0f;
    }
}
