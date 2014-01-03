using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace BotEntitiesPlugin
{
    public class BotEntitiesPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get { return "BotEntities"; }
        }

        public List<string> PluginDependencies
        {
            get { return new List<string> { "Renderable", "Motion", "EventLoop"}; }
        }

        public List<string> ComponentDependencies
        {
            get { return new List<string> { "meshResource", "velocity", "rotationalVelocity" };  }
        }

        public void Initialize()
        {
           CreateBotEntities();
        }

        private void RetrieveConfigurationValues()
        {
            string configPath = this.GetType().Assembly.Location;
            Configuration config = ConfigurationManager.OpenExeConfiguration(configPath);

            // TODO: Parse values
        }

        private void CreateBotEntities()
        {
        }


        private int numBots = 10;
        private string botMesh = "models/natalieFives/xml3d/natalie.xml";
        private float botWalkSpeed = 0.05f;
        private float botRotateSpeed = 0.05f;
    }
}
