using System;
using System.Collections.Generic;
using FIVES;
using KIARA;
using Math;

namespace Motion
{
    public class MotionPlugin : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string GetName()
        {
            return "Motion";
        }

        public List<string> GetDependencies()
        {
            return new List<string> { "Location" };
        }

        public void Initialize()
        {
            DefineComponents();
            RegisterClientServices();
        }

        #endregion

        void DefineComponents()
        {
            // Velocity is represented as a vector (x,y,z) in world units per second.
            ComponentLayout velocityLayout = new ComponentLayout();
            velocityLayout.AddAttribute<float> ("x", 0f);
            velocityLayout.AddAttribute<float> ("y", 0f);
            velocityLayout.AddAttribute<float> ("z", 0f);

            // Rotation velocity is represented as an axis (x, y, z) and angular rotation r in radians per second.
            ComponentLayout rotVelocityLayout = new ComponentLayout();
            rotVelocityLayout.AddAttribute<float>("x", 0f);
            rotVelocityLayout.AddAttribute<float>("y", 0f);
            rotVelocityLayout.AddAttribute<float>("z", 0f);
            rotVelocityLayout.AddAttribute<float>("r", 1f);

            ComponentRegistry.Instance.DefineComponent("velocity", pluginGUID, velocityLayout);
            ComponentRegistry.Instance.DefineComponent("rotVelocity", pluginGUID, rotVelocityLayout);
        }

        void RegisterClientServices()
        {
            PluginManager.Instance.AddPluginLoadedHandler("ClientManager", delegate {
                var interPluginContext = ContextFactory.GetContext("inter-plugin");
                var clientManager = ServiceFactory.DiscoverByName("clientmanager", interPluginContext);
                clientManager.OnConnected += delegate(Connection connection) {
                    connection["registerClientService"]("motion", true, new Dictionary<string, Delegate> {
                        {"update", (Action<string, Vector, RotVelocity, int>) Update},
                    });
                };
            });
        }

        private void Update(string guid, Vector velocity, RotVelocity rotVelocity, int timestamp)
        {
            var entity = EntityRegistry.Instance.GetEntity(guid);
            entity["velocity"]["x"] = velocity.x;
            entity["velocity"]["y"] = velocity.y;
            entity["velocity"]["z"] = velocity.z;
            entity["rotVelocity"]["x"] = rotVelocity.axis.x;
            entity["rotVelocity"]["y"] = rotVelocity.axis.y;
            entity["rotVelocity"]["z"] = rotVelocity.axis.z;
            entity["rotVelocity"]["r"] = rotVelocity.rotSpeed;

            // We currently ignore timestamp, but may it in the future to implement dead reckoning.
        }

        private readonly Guid pluginGUID = new Guid("bd5b8634-890c-4f59-a823-f9d2b1fd0c86");
    }
}

