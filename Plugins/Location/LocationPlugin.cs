using System;
using FIVES;
using System.Collections.Generic;
using KIARA;

namespace Location
{
    /// <summary>
    /// Plugin that registers two components - position and orientation. Does not provide any associated functionality.
    /// </summary>
    public class LocationPlugin : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string GetName()
        {
            return "Location";
        }

        public List<string> GetDependencies()
        {
            return new List<string>();
        }

        public void Initialize()
        {
            DefineComponents();
            RegisterClientServices();
        }

        #endregion

        void DefineComponents()
        {
            // Position is represented as a vector (x,y,z) from the default position, which is at (0,0,0).
            ComponentLayout positionLayout = new ComponentLayout();
            positionLayout.AddAttribute<float> ("x", 0f);
            positionLayout.AddAttribute<float> ("y", 0f);
            positionLayout.AddAttribute<float> ("z", 0f);

            // Orientation is represented as a quaternion, where (x,y,z) is a vector part, and w is a scalar part. The 
            // orientation of the object is relative to the default orientation. In the default position and 
            // orientation, the viewer is on the Z-axis looking down the -Z-axis toward the origin with +X to the right 
            // and +Y straight up.
            ComponentLayout orientationLayout = new ComponentLayout();
            orientationLayout.AddAttribute<float>("x", 0f);
            orientationLayout.AddAttribute<float>("y", 0f);
            orientationLayout.AddAttribute<float>("z", 0f);
            orientationLayout.AddAttribute<float>("w", 1f);

            ComponentRegistry.Instance.DefineComponent("position", pluginGUID, positionLayout);
            ComponentRegistry.Instance.DefineComponent("orientation", pluginGUID, orientationLayout);
        }

        void RegisterClientServices()
        {
            PluginManager.Instance.AddPluginLoadedHandler("ClientSync", delegate {
                var interPluginContext = ContextFactory.GetContext("inter-plugin");
                var clientManager = ServiceFactory.DiscoverByName("clientmanager", interPluginContext);
                clientManager.OnConnected += delegate(Connection connection) {
                    connection["registerClientService"]("location", true, new Dictionary<string, Delegate> {
                        {"update", (Action<string, Vector, Quat, int>) Update},
                        {"notifyAboutUpdates", (Action<string, Action<Vector, Quat>>) NotifyAboutUpdates},
                    });
                };
            });
        }

        private void Update(string guid, Vector position, Quat orientation, int timestamp)
        {
            var entity = EntityRegistry.Instance.GetEntity(guid);
            entity["position"]["x"] = position.x;
            entity["position"]["y"] = position.y;
            entity["position"]["z"] = position.z;
            entity["orientation"]["x"] = orientation.x;
            entity["orientation"]["y"] = orientation.y;
            entity["orientation"]["z"] = orientation.z;
            entity["orientation"]["w"] = orientation.w;

            // We currently ignore timestamp, but may it in the future to implement dead reckoning.
        }

        private void NotifyAboutUpdates(string guid, Action<Vector, Quat> callback)
        {
            var entity = EntityRegistry.Instance.GetEntity(guid);
            var attributeChangeHandler = new Component.AttributeChanged((sender, ev) => {
                Vector position = new Vector {
                    x = (float)entity["position"]["x"],
                    y = (float)entity["position"]["y"],
                    z = (float)entity["position"]["z"],
                };

                Quat orientation = new Quat {
                    x = (float)entity["orientation"]["x"],
                    y = (float)entity["orientation"]["y"],
                    z = (float)entity["orientation"]["z"],
                    w = (float)entity["orientation"]["w"],
                };

                callback(position, orientation);
            });

            entity["position"].OnAttributeChanged += attributeChangeHandler;
            entity["orientation"].OnAttributeChanged += attributeChangeHandler;
        }

        private readonly Guid pluginGUID = new Guid("90dd4c50-f09d-11e2-b778-0800200c9a66");
    }
}

