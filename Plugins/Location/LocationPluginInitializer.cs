using System;
using FIVES;
using System.Collections.Generic;
using ClientManagerPlugin;

namespace LocationPlugin
{
    /// <summary>
    /// Plugin that registers two components - position and orientation. Does not provide any associated functionality.
    /// </summary>
    public class LocationPluginInitializer : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string Name
        {
            get
            {
                return "Location";
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
            DefineComponents();

            PluginManager.Instance.AddPluginLoadedHandler("ClientManager", RegisterClientServices);
        }

        public void Shutdown()
        {
        }

        #endregion

        void DefineComponents()
        {
            // Position is represented as a vector (x,y,z) from the default position, which is at (0,0,0).
            ComponentDefinition position = new ComponentDefinition("position");
            position.AddAttribute<float> ("x", 0f);
            position.AddAttribute<float> ("y", 0f);
            position.AddAttribute<float> ("z", 0f);
            ComponentRegistry.Instance.Register(position);

            // Orientation is represented as a quaternion, where (x,y,z) is a vector part, and w is a scalar part. The 
            // orientation of the object is relative to the default orientation. In the default position and 
            // orientation, the viewer is on the Z-axis looking down the -Z-axis toward the origin with +X to the right 
            // and +Y straight up.
            ComponentDefinition orientation = new ComponentDefinition("orientation");
            orientation.AddAttribute<float>("x", 0f);
            orientation.AddAttribute<float>("y", 0f);
            orientation.AddAttribute<float>("z", 0f);
            orientation.AddAttribute<float>("w", 1f);
            ComponentRegistry.Instance.Register(orientation);
        }

        void RegisterClientServices()
        {
            ClientManager.Instance.RegisterClientService("location", true, new Dictionary<string, Delegate> {
                {"updatePosition", (Action<string, string, Vector, int>) UpdatePosition},
                {"updateOrientation", (Action<string, string, Quat, int>) UpdateOrientation}
            });
        }

        private void UpdatePosition(string sessionKey, string guid, Vector position, int timestamp)
        {
            var entity = World.Instance.FindEntity(guid);
            entity["position"]["x"] = position.x;
            entity["position"]["y"] = position.y;
            entity["position"]["z"] = position.z;

            // We currently ignore timestamp, but may it in the future to implement dead reckoning.
        }

        private void UpdateOrientation(string sessionKey, string guid, Quat orientation, int timestamp)
        {
            var entity = World.Instance.FindEntity(guid);
            entity["orientation"]["x"] = orientation.x;
            entity["orientation"]["y"] = orientation.y;
            entity["orientation"]["z"] = orientation.z;
            entity["orientation"]["w"] = orientation.w;

            // We currently ignore timestamp, but may it in the future to implement dead reckoning.
        }
    }
}

