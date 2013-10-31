using System;
using System.Collections.Generic;
using FIVES;
using ClientManagerPlugin;
using KIARA;
using FiVESMath;
using System.Threading;
using Events;

namespace MotionPlugin
{
    public class MotionPluginInitializer : IPluginInitializer
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
            RegisterEntityEvents();
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

                ClientManager.Instance.RegisterClientService("motion", true, new Dictionary<string, Delegate> {
                        {"update", (Action<string, Vector, RotVelocity, int>) Update},
                        {"startMotion", (Action<string>)StartMotion},
                        {"stopMotion", (Action<string>)StopMotion}
                    });
                };
            });
        }

        /// <summary>
        /// Registers to Events fired by entities
        /// </summary>
        private void RegisterEntityEvents()
        {
            registerToExistingEntities();
            EntityRegistry.Instance.OnEntityAdded += new EntityRegistry.EntityAdded(handleOnNewEntity);
        }

        /// <summary>
        /// Traverses the entity registry and registers the handler for changed attributes on each of them
        /// </summary>
        private void registerToExistingEntities()
        {
            HashSet<Guid> existingEntities = EntityRegistry.Instance.GetAllGUIDs();
            foreach (Guid g in existingEntities)
            {
                Entity entity = EntityRegistry.Instance.GetEntity(g);
                entity.OnAttributeInComponentChanged += new Entity.AttributeInComponentChanged(handleOnAttributeChanged);
            }
        }

        /// <summary>
        /// Handles a New Entity Event of the EntityRegistry and registers the handler for attribute changes on this entity
        /// </summary>
        /// <param name="sender">The Entity Registry</param>
        /// <param name="e">The Event Parameters</param>
        private void handleOnNewEntity(Object sender, EntityAddedOrRemovedEventArgs e)
        {
            Entity entity = EntityRegistry.Instance.GetEntity(e.elementId);
            entity.OnAttributeInComponentChanged += new Entity.AttributeInComponentChanged(handleOnAttributeChanged);
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

        private void UpdateMotion(string guid) {
            Entity updatedEntity = EntityRegistry.Instance.GetEntity(guid);

            while (ongoingMotion.Contains(guid))
            {
                updatedEntity["position"]["x"] = (float)updatedEntity["position"]["x"] + (float)updatedEntity["velocity"]["x"];
                updatedEntity["position"]["y"] = (float)updatedEntity["position"]["y"] + (float)updatedEntity["velocity"]["y"];
                updatedEntity["position"]["z"] = (float)updatedEntity["position"]["z"] + (float)updatedEntity["velocity"]["z"];
            }
        }

        private void StartMotion(string guid)
        {
            if (!ongoingMotion.Contains(guid))
            {
                ongoingMotion.Add(guid);
                ThreadPool.QueueUserWorkItem(_ => UpdateMotion(guid));
            }
        }

        private void StopMotion(string guid)
        {
            if (ongoingMotion.Contains(guid))
                ongoingMotion.Remove(guid);
        }

        private ISet<string> ongoingMotion = new HashSet<string>();
        private readonly Guid pluginGUID = new Guid("bd5b8634-890c-4f59-a823-f9d2b1fd0c86");
    }
}

