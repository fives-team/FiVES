// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
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
                return new List<string> {"EventLoop"};
            }
        }

        public List<string> ComponentDependencies
        {
            get
            {
                return new List<string> { "location" };
            }
        }

        /// <summary>
        /// Initializes the plugin by registering Components, subscribing to events and accessing other plugins
        /// </summary>
        public void Initialize()
        {
            RegisterToECA();
            RegisterToPluginEvents();
        }

        /// <summary>
        /// Registers Components to ECA and subscribes to Entity events
        /// </summary>
        internal void RegisterToECA()
        {
            DefineComponents();
            RegisterEntityEvents();
        }

        /// <summary>
        /// Subscribes to EventLoop events and ClientManager PluginInLoaded handler to register client services
        /// </summary>
        private void RegisterToPluginEvents()
        {
            EventLoop.Instance.TickFired += new EventHandler<TickEventArgs>(HandleEventTick);
            PluginManager.Instance.AddPluginLoadedHandler("ClientManager", RegisterClientServices);
        }

        public void Shutdown()
        {
        }

        #endregion

        void DefineComponents()
        {
            ComponentDefinition motion = new ComponentDefinition("motion");
            motion.AddAttribute<Vector>("velocity", new Vector(0, 0, 0));
            motion.AddAttribute<AxisAngle>("rotVelocity", new AxisAngle(0, 1, 0, 0));
            ComponentRegistry.Instance.Register(motion);
        }

        void RegisterClientServices()
        {
            ClientManager.Instance.RegisterClientService("motion", true, new Dictionary<string, Delegate> {
                {"update", (Action<string, Vector, AxisAngle, int>) Update}
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

        private void Update(string guid, Vector velocity, AxisAngle rotVelocity, int timestamp)
        {
            var entity = World.Instance.FindEntity(guid);
            entity["motion"]["velocity"] = velocity;
            entity["motion"]["rotVelocity"] = rotVelocity;

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
            CheckForEntityMoving(entity);
            CheckForEntitySpinning(entity);
        }

        /// <summary>
        /// Checks if an attribute update of an entity initiated or ended a movement by checking its new velocity values. Adds or removes the entity
        /// to the list of ongoing motions depending on the values.
        /// </summary>
        /// <param name="entity">Entity that fired attribute changed event</param>
        private void CheckForEntityMoving(Entity entity)
        {
            if (IsMoving(entity))
            {
                lock (ongoingMotion)
                {
                    if (!ongoingMotion.Contains(entity))
                        ongoingMotion.Add(entity);
                }
            }
            else
            {
                lock (ongoingMotion)
                {
                    if(ongoingMotion.Contains(entity))
                        ongoingMotion.Remove(entity);
                }
            }
        }

        /// <summary>
        /// Checks if an attribute update of an entity initiated or ended a spin by checking its new rotational velocity values. Adds or removes the entity
        /// to the list of ongoing spins depending on the values.
        /// </summary>
        /// <param name="entity">Entity that fired attribute changed event</param>
        private void CheckForEntitySpinning(Entity entity)
        {
            if (IsSpinning(entity))
            {
                lock (ongoingSpin)
                {
                    if (!ongoingSpin.Contains(entity))
                        ongoingSpin.Add(entity);
                }
            }
            else
            {
                lock (ongoingSpin)
                {
                    if (ongoingSpin.Contains(entity))
                        ongoingSpin.Remove(entity);
                }
            }
        }

        /// <summary>
        /// Handles a TickFired Evenet of EventLoop. Performs position and orientation updates for all ongoing motions and rotations
        /// </summary>
        /// <param name="sender">Sender of tick event args (EventLoop)</param>
        /// <param name="e">TickEventArgs</param>
        private void HandleEventTick(Object sender, TickEventArgs e)
        {
            lock (ongoingMotion)
            {
                foreach (Entity entity in ongoingMotion)
                {
                    UpdateMotion(entity);
                }
            }

            lock (ongoingSpin)
            {
                foreach (Entity entity in ongoingSpin)
                    UpdateSpin(entity);
            }
        }

        /// <summary>
        /// Worker Thread function that periodically performs the motion. Ends, when velocity of entity is 0
        /// </summary>
        /// <param name="updatedEntity">Entity for which motion is updated</param>
        internal void UpdateMotion(Entity updatedEntity) {
            Vector localVelocity = GetVelocityInWorldSpace(updatedEntity);
            Vector oldPosition = (Vector)updatedEntity["location"]["position"];
            updatedEntity["location"]["position"] = new Vector(
                oldPosition.x + localVelocity.x,
                oldPosition.y + localVelocity.y,
                oldPosition.z + localVelocity.z
            );
        }

        /// <summary>
        /// Worker thread that periodically performs the spin. Ends, when either angular velocity or spin axis are 0.
        /// </summary>
        /// <param name="updatedEntity">Entity for which spin is updated</param>
        internal void UpdateSpin(Entity updatedEntity)
        {
            Quat entityRotation = EntityRotationAsQuaternion(updatedEntity);

            AxisAngle rotVelocity = (AxisAngle)updatedEntity["motion"]["rotVelocity"];

            Quat spinAsQuaternion = FIVES.Math.QuaternionFromAxisAngle(rotVelocity.axis, rotVelocity.angle);

            Quat newRotationAsQuaternion = FIVES.Math.MultiplyQuaternions(spinAsQuaternion, entityRotation);
            updatedEntity["location"]["orientation"] = newRotationAsQuaternion;
        }

        /// <summary>
        /// Converts velocity from entity's to world coordinate system
        /// </summary>
        /// <param name="updatedEntity">Entity to convert velocity from</param>
        /// <returns>Entity's velocity in world coordinates</returns>
        private Vector GetVelocityInWorldSpace(Entity updatedEntity)
        {
            Vector velocity = (Vector)updatedEntity["motion"]["velocity"];
            Quat entityRotation = (Quat)updatedEntity["location"]["orientation"];

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
            return (Quat)entity["location"]["orientation"];
        }

        /// <summary>
        /// Checks if the entity has a non 0 velocity
        /// </summary>
        /// <param name="entity">Entity to check</param>
        /// <returns>true, if at least one attribute of its velocity component is != 0</returns>
        private bool IsMoving(Entity entity) {
            var velocity = (Vector)entity["motion"]["velocity"];
            return velocity.x != 0 || velocity.y != 0 || velocity.z != 0;
        }

        /// <summary>
        /// Checks if an entity is currently spinning. An entity is not spinning if either the axis or the angular velocity are 0.
        /// </summary>
        /// <param name="entity">Entity to check</param>
        /// <returns>True, if spin axis is not the null vector, and velocity is not 0</returns>
        private bool IsSpinning(Entity entity) {
            var rotVelocity = (AxisAngle)entity["motion"]["rotVelocity"];
            return rotVelocity.angle != 0 &&
                !(rotVelocity.axis.x == 0 && rotVelocity.axis.y == 0 && rotVelocity.axis.z == 0);
        }

        private ISet<Entity> ongoingMotion = new HashSet<Entity>();
        private ISet<Entity> ongoingSpin = new HashSet<Entity>();
    }
}

