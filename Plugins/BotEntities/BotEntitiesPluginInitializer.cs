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

            int.TryParse(ConfigurationManager.AppSettings["numBots"], out numBots);
            botMesh = ConfigurationManager.AppSettings["botMesh"];
            float.TryParse(ConfigurationManager.AppSettings["walkSpeed"], out botWalkSpeed);
            float.TryParse(ConfigurationManager.AppSettings["rotateSpeed"], out botRotateSpeed);
            int.TryParse(ConfigurationManager.AppSettings["updateInterval"], out botUpdateInterval);
        }

        private void CreateBotEntities()
        {
            for (var i = 0; i < numBots; i++)
            {
                Entity botEntity = new Entity();
                botEntity["position"]["x"] = 0.0f;
                botEntity["position"]["y"] = 0.0f;
                botEntity["position"]["z"] = 0.0f;

                botEntity["rotVelocity"]["x"] = 0f;
                botEntity["rotVelocity"]["y"] = 1f;
                botEntity["rotVelocity"]["z"] = 0f;
                botEntity["rotVelocity"]["r"] = 0f;

                botEntity["meshResource"]["uri"] = botMesh;
                FIVES.World.Instance.Add(botEntity);
                bots.Add(botEntity);
            }
        }

        private void HandleEventTick(Object sender, TickEventArgs e)
        {
            millisecondsSinceLastTick += EventLoop.Instance.TickInterval;
            if (millisecondsSinceLastTick > 1000)
            {
                foreach (Entity botEntity in bots)
                {
                    int moveRoll = random.Next(10);
                    if (moveRoll > 8)
                        ChangeMoveSpeed(botEntity);
                    if (moveRoll < 2)
                        ChangeRotateSpeed(botEntity);
                }
            }
        }

        private void ChangeMoveSpeed(Entity botEntity)
        {
            if ((float)botEntity["velocity"]["x"] == botWalkSpeed)
                botEntity["velocity"]["x"] = 0.0f;
            else
                botEntity["velocity"]["x"] = botWalkSpeed;
        }

        private void ChangeRotateSpeed(Entity botEntity)
        {
            int rotateRoll = random.Next(10);
            if (rotateRoll == 8)
                botEntity["rotVelocity"]["r"] = botRotateSpeed;
            else if (rotateRoll == 9)
                botEntity["rotVelocity"]["r"] = -botRotateSpeed;
            else
                botEntity["rotVelocity"]["r"] = 0f;
        }

        private int numBots = 10;
        private HashSet<Entity> bots = new HashSet<Entity>();
        private string botMesh = "/models/natalieFives/xml3d/natalie.xml";
        private float botWalkSpeed = 0.05f;
        private float botRotateSpeed = 0.05f;
        private int millisecondsSinceLastTick = 0;
        private Random random = new Random();
    }
}
