// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using FIVES;
using ClientManagerPlugin;
using System.Threading;
using EventLoopPlugin;
using System.IO;
using FIVESServiceBus;

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
            DefineComponents();
            RegisterServiceBusService();
            RegisterToPluginEvents();
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

        internal void DefineComponents()
        {
            ComponentDefinition motion = new ComponentDefinition("motion");
            motion.AddAttribute<Vector>("velocity", new Vector(0, 0, 0));
            motion.AddAttribute<AxisAngle>("rotVelocity", new AxisAngle(0, 1, 0, 0));
            ComponentRegistry.Instance.Register(motion);
        }

        internal void RegisterServiceBusService()
        {
            ServiceBus.ServiceRegistry.RegisterService("InvokeMotion", InvokeMotion);
            ServiceBus.ServiceRegistry.RegisterService("InvokeSpin", InvokeSpin);
        }

        void RegisterClientServices()
        {
            string motionIdl = File.ReadAllText("motion.kiara");
            SINFONIPlugin.SINFONIServerManager.Instance.SinfoniServer.AmendIDL(motionIdl);
            ClientManager.Instance.RegisterClientService("motion", true, new Dictionary<string, Delegate> {
                {"update", (Action<string, Vector, AxisAngle, int>) Update}
            });
        }

        private void Update(string guid, Vector velocity, AxisAngle rotVelocity, int timestamp)
        {
            var entity = World.Instance.FindEntity(guid);
            entity["motion"]["velocity"].Suggest(velocity);
            entity["motion"]["rotVelocity"].Suggest(rotVelocity);
            // We currently ignore timestamp, but may it in the future to implement dead reckoning.
        }

        private AccumulatedAttributeTransform InvokeMotion(AccumulatedAttributeTransform accumulatedTransforms)
        {
            var velocity = (Vector)accumulatedTransforms.CurrentAttributeValue("motion", "velocity");

            if(velocity.x != 0 || velocity.y != 0 || velocity.z != 0)
            {
                lock (ongoingMotion)
                {
                    if (!ongoingMotion.Contains(accumulatedTransforms.Entity))
                        ongoingMotion.Add(accumulatedTransforms.Entity);
                }
            }
            else
            {
                lock (ongoingMotion)
                {
                    if (ongoingMotion.Contains(accumulatedTransforms.Entity))
                        ongoingMotion.Remove(accumulatedTransforms.Entity);
                }
            }
            return accumulatedTransforms;
        }

        private AccumulatedAttributeTransform InvokeSpin(AccumulatedAttributeTransform accumulatedTransforms)
        {
            var rotVelocity = (AxisAngle)accumulatedTransforms.CurrentAttributeValue("motion", "rotVelocity");

            if (rotVelocity.angle != 0 &&
                !(rotVelocity.axis.x == 0 && rotVelocity.axis.y == 0 && rotVelocity.axis.z == 0))
            {
                lock (ongoingSpin)
                {
                    if (!ongoingSpin.Contains(accumulatedTransforms.Entity))
                        ongoingSpin.Add(accumulatedTransforms.Entity);
                }
            }
            else
            {
                lock (ongoingSpin)
                {
                    if (ongoingSpin.Contains(accumulatedTransforms.Entity))
                        ongoingSpin.Remove(accumulatedTransforms.Entity);
                }
            }

            return accumulatedTransforms;
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
            Vector oldPosition = (Vector)updatedEntity["location"]["position"].Value;
            Vector newPosition = new Vector(
                oldPosition.x + localVelocity.x,
                oldPosition.y + localVelocity.y,
                oldPosition.z + localVelocity.z
            );
            updatedEntity["location"]["position"].Suggest(newPosition);
        }

        /// <summary>
        /// Worker thread that periodically performs the spin. Ends, when either angular velocity or spin axis are 0.
        /// </summary>
        /// <param name="updatedEntity">Entity for which spin is updated</param>
        internal void UpdateSpin(Entity updatedEntity)
        {
            Quat entityRotation = EntityRotationAsQuaternion(updatedEntity);

            AxisAngle rotVelocity = (AxisAngle)updatedEntity["motion"]["rotVelocity"].Value;

            Quat spinAsQuaternion = FIVES.Math.QuaternionFromAxisAngle(rotVelocity.axis, rotVelocity.angle);

            Quat newRotationAsQuaternion = FIVES.Math.MultiplyQuaternions(spinAsQuaternion, entityRotation);
            updatedEntity["location"]["orientation"].Suggest(newRotationAsQuaternion);
        }

        /// <summary>
        /// Converts velocity from entity's to world coordinate system
        /// </summary>
        /// <param name="updatedEntity">Entity to convert velocity from</param>
        /// <returns>Entity's velocity in world coordinates</returns>
        private Vector GetVelocityInWorldSpace(Entity updatedEntity)
        {
            Vector velocity = (Vector)updatedEntity["motion"]["velocity"].Value;
            Quat entityRotation = (Quat)updatedEntity["location"]["orientation"].Value;

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
            return (Quat)entity["location"]["orientation"].Value;
        }

        private ISet<Entity> ongoingMotion = new HashSet<Entity>();
        private ISet<Entity> ongoingSpin = new HashSet<Entity>();
    }
}

