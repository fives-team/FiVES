using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using EventLoopPlugin;

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
            get { return new List<string> { "EventLoop"}; }
        }

        public List<string> ComponentDependencies
        {
            get { return new List<string> { "meshResource", "velocity", "rotVelocity" };  }
        }

        public void Initialize()
        {
           CreateBotEntities();
           EventLoop.Instance.TickFired += new EventHandler<TickEventArgs>(HandleEventTick);
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

        private void HandleEventTick(Object sender, TickEventArgs e)
        {
        }

        private int numBots = 10;
        private HashSet<Entity> bots = new HashSet<Entity>();
        private string botMesh = "models/natalieFives/xml3d/natalie.xml";
        private float botWalkSpeed = 0.05f;
        private float botRotateSpeed = 0.05f;
    }
}
