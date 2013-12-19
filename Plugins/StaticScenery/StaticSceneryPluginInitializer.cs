using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FIVES;

namespace StaticSceneryPlugin
{
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

        private void ReadConfig()
        {
            string sceneryConfigPath = this.GetType().Assembly.Location;
            Configuration config = ConfigurationManager.OpenExeConfiguration(sceneryConfigPath);

            SceneryURL = config.AppSettings.Settings["sceneryUrl"].Value;
            string offsetYConfigValue = config.AppSettings.Settings["offsetY"].Value;
            float.TryParse(offsetYConfigValue, out OffsetY);
        }

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
