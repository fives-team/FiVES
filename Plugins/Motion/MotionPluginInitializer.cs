using System;
using System.Collections.Generic;
using FIVES;
using ClientManagerPlugin;
using System.Threading;
using EventLoopPlugin;

namespace MotionPlugin
{
    public class MotionPluginInitializer : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string Name
        {
            get
            {
                return "Motion";
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
                return new List<string> { "position", "orientation" };
            }
        }

        public void Initialize()
        {
            DefineComponents();
            RegisterEntityEvents();

            PluginManager.Instance.AddPluginLoadedHandler("ClientManager", RegisterClientServices);
        }

        #endregion

        void DefineComponents()
        {
            // Velocity is represented as a vector (x,y,z) in world units per second.
            ComponentDefinition velocity = new ComponentDefinition("velocity");
            velocity.AddAttribute<float> ("x", 0f);
            velocity.AddAttribute<float> ("y", 0f);
            velocity.AddAttribute<float> ("z", 0f);
            ComponentRegistry.Instance.Register(velocity);

            // Rotation velocity is represented as an axis (x, y, z) and angular rotation r in radians per second.
            ComponentDefinition rotVelocity = new ComponentDefinition("rotVelocity");
            rotVelocity.AddAttribute<float>("x", 0f);
            rotVelocity.AddAttribute<float>("y", 1f);
            rotVelocity.AddAttribute<float>("z", 0f);
            rotVelocity.AddAttribute<float>("r", 0f);
            ComponentRegistry.Instance.Register(rotVelocity);
        }

        void RegisterClientServices()
        {
            ClientManager.Instance.RegisterClientService("motion", true, new Dictionary<string, Delegate> {
                {"update", (Action<string, Vector, RotVelocity, int>) Update}
            });
        }

        /// <summary>
        /// Registers to Events fired by entities
        /// </summary>
        private void RegisterEntityEvents()
        {
            RegisterToExistingEntities();
            World.Instance.AddedEntity += new EventHandler<EntityEventArgs>(HandleOnNewEntity);
        }

        /// <summary>
        /// Traverses the entity registry and registers the handler for changed attributes on each of them
        /// </summary>
        private void RegisterToExistingEntities()
        {
            foreach (Entity entity in World.Instance)
            {
                entity.ChangedAttribute +=
                    new EventHandler<ChangedAttributeEventArgs>(HandleOnAttributeChanged);
            }
        }

        /// <summary>
        /// Handles a New Entity Event of the EntityRegistry and registers the handler for attribute changes on this entity
        /// </summary>
        /// <param name="sender">The Entity Registry</param>
        /// <param name="e">The Event Parameters</param>
        private void HandleOnNewEntity(Object sender, EntityEventArgs e)
        {
            e.Entity.ChangedAttribute += new EventHandler<ChangedAttributeEventArgs>(HandleOnAttributeChanged);
        }

        private void Update(string guid, Vector velocity, RotVelocity rotVelocity, int timestamp)
        {
            var entity = World.Instance.FindEntity(guid);
            entity["velocity"]["x"] = velocity.x;
            entity["velocity"]["y"] = velocity.y;
            entity["velocity"]["z"] = velocity.z;
            entity["rotVelocity"]["x"] = rotVelocity.axis.x;
            entity["rotVelocity"]["y"] = rotVelocity.axis.y;
            entity["rotVelocity"]["z"] = rotVelocity.axis.z;
            entity["rotVelocity"]["r"] = rotVelocity.rotSpeed;

            // We currently ignore timestamp, but may it in the future to implement dead reckoning.
        }

        /// <summary>
        /// Handles the AttributeInComponentChanged-Event of an Entity. Invokes or stops a motion depending on the new values for velocity
        /// </summary>
        /// <param name="sender">The entity that fired the event</param>
        /// <param name="e">The EventArgs</param>
        private void HandleOnAttributeChanged(Object sender, ChangedAttributeEventArgs e)
        {
            Entity entity = (Entity)sender;
            if (IsMoving(entity) && !ongoingMotion.Contains(entity.Guid))
            {
                ongoingMotion.Add(entity.Guid);
                ThreadPool.QueueUserWorkItem(_ => UpdateMotion(entity));
            }
            if (IsSpinning(entity) && !ongoingSpin.Contains(entity.Guid))
            {
                ongoingSpin.Add(entity.Guid);
                ThreadPool.QueueUserWorkItem(_ => UpdateSpin(entity));
            }
        }

        /// <summary>
        /// Worker Thread function that periodically performs the motion. Ends, when velocity of entity is 0
        /// </summary>
        /// <param name="updatedEntity">Entity for which motion is updated</param>
        private void UpdateMotion(Entity updatedEntity) {
            while (IsMoving(updatedEntity))
            {
                Vector localVelocity = GetVelocityInWorldSpace(updatedEntity);
                updatedEntity["position"]["x"] = (float)updatedEntity["position"]["x"] + localVelocity.x;
                updatedEntity["position"]["y"] = (float)updatedEntity["position"]["y"] + localVelocity.y;
                updatedEntity["position"]["z"] = (float)updatedEntity["position"]["z"] + localVelocity.z;
                Thread.Sleep(30);
            }
            ongoingMotion.Remove(updatedEntity.Guid);
        }

        /// <summary>
        /// Worker thread that periodically performs the spin. Ends, when either angular velocity or spin axis are 0.
        /// </summary>
        /// <param name="updatedEntity">Entity for which spin is updated</param>
        private void UpdateSpin(Entity updatedEntity) {
            while (IsSpinning(updatedEntity))
            {
                Quat entityRotation = EntityRotationAsQuaternion(updatedEntity);

                Vector spinAxis = new Vector();
                spinAxis.x = (float)updatedEntity["rotVelocity"]["x"];
                spinAxis.y = (float)updatedEntity["rotVelocity"]["y"];
                spinAxis.z = (float)updatedEntity["rotVelocity"]["z"];
                float spinAngle = (float)updatedEntity["rotVelocity"]["r"];

                Quat spinAsQuaternion = FIVES.Math.QuaternionFromAxisAngle(spinAxis, spinAngle);

                Quat newRotationAsQuaternion = FIVES.Math.MultiplyQuaternions(spinAsQuaternion, entityRotation);
                updatedEntity["orientation"]["x"] = newRotationAsQuaternion.x;
                updatedEntity["orientation"]["y"] = newRotationAsQuaternion.y;
                updatedEntity["orientation"]["z"] = newRotationAsQuaternion.z;
                updatedEntity["orientation"]["w"] = newRotationAsQuaternion.w;

                Thread.Sleep(30);
            }
            ongoingSpin.Remove(updatedEntity.Guid);
        }

        /// <summary>
        /// Converts velocity from entity's to world coordinate system
        /// </summary>
        /// <param name="updatedEntity">Entity to convert velocity from</param>
        /// <returns>Entity's velocity in world coordinates</returns>
        private Vector GetVelocityInWorldSpace(Entity updatedEntity)
        {
            Vector velocity = new Vector();
            velocity.x = (float)updatedEntity["velocity"]["x"];
            velocity.y = (float)updatedEntity["velocity"]["y"];
            velocity.z = (float)updatedEntity["velocity"]["z"];

            Quat entityRotation = new Quat();
            entityRotation.x = (float)updatedEntity["orientation"]["x"];
            entityRotation.y = (float)updatedEntity["orientation"]["y"];
            entityRotation.z = (float)updatedEntity["orientation"]["z"];
            entityRotation.w = (float)updatedEntity["orientation"]["w"];

            Vector axis = FIVES.Math.AxisFromQuaternion(entityRotation);
            float angle = FIVES.Math.AngleFromQuaternion(entityRotation);

            return FIVES.Math.RotateVectorByAxisAngle(velocity, axis, -angle) /* negative angle because we apply the inverse transform */;
        }

        /// <summary>
        /// Helper function to convert an entity's orientation component directly to a Quat
        /// </summary>
        /// <param name="entity">Entity to get orientation from</param>
        /// <returns></returns>
        private Quat EntityRotationAsQuaternion(Entity entity) {
            Quat entityRotation = new Quat();
            entityRotation.x = (float)entity["orientation"]["x"];
            entityRotation.y = (float)entity["orientation"]["y"];
            entityRotation.z = (float)entity["orientation"]["z"];
            entityRotation.w = (float)entity["orientation"]["w"];

            return entityRotation;
        }

        /// <summary>
        /// Checks if the entity has a non 0 velocity
        /// </summary>
        /// <param name="entity">Entity to check</param>
        /// <returns>true, if at least one attribute of its velocity component is != 0</returns>
        private bool IsMoving(Entity entity) {
            return !((float)entity["velocity"]["x"] == 0
                && (float)entity["velocity"]["y"] == 0
                && (float)entity["velocity"]["z"] == 0);
        }

        /// <summary>
        /// Checks if an entity is currently spinning. An entity is not spinning if either the axis or the angular velocity are 0.
        /// </summary>
        /// <param name="entity">Entity to check</param>
        /// <returns>True, if spin axis is not the null vector, and velocity is not 0</returns>
        private bool IsSpinning(Entity entity) {
            return (float)entity["rotVelocity"]["r"] != 0
                && !((float)entity["rotVelocity"]["x"] == 0
                    && (float)entity["rotVelocity"]["y"] == 0
                    && (float)entity["rotVelocity"]["z"] ==0);
        }

        private ISet<Entity> ongoingMotion = new HashSet<Entity>();
        private ISet<Entity> ongoingSpin = new HashSet<Entity>();
    }
}

