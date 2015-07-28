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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientManagerPlugin;
using FIVES;
using FIVESServiceBus;
using System.IO;
using SINFONIPlugin;

namespace AvatarCollisionPlugin
{
    /// <summary>
    /// AvatarCollision introduces an attribute that specifies the current ground level for an avatar as determined
    /// by a client. If the groundlevel is set (may not be the case for non-avatar entities), the entitiy is
    /// automatically put to this height. Collision with geometry in moving direction of the avatar is handled by
    /// the client setting velocity to 0. Having the height of the avatar set on the server avoids sending incorrect
    /// collision values to the client which then have to be resent to the server in corrected form. Instead, the
    /// server checks for correct position and sends a correct vector immediately.
    /// </summary>
    public class AvatarCollisionPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get { return "AvatarCollision"; }
        }

        public List<string> PluginDependencies
        {
            get { return new List<string> {"SINFONI"}; }
        }

        public List<string> ComponentDependencies
        {
            get { return new List<string> {"location"}; }
        }

        public void Initialize()
        {
            RegisterComponents();
            RegisterToEvents();

            ServiceBus.ServiceRegistry.RegisterService("AvatarCollision", Transform);
            PluginManager.Instance.AddPluginLoadedHandler("ClientManager", RegisterService);
        }

        public void Shutdown()
        {
        }

        /// <summary>
        /// Registers gravity component that carries the attribute for the groundlevel
        /// </summary>
        internal void RegisterComponents() {
            ComponentDefinition gravityDefinition = new ComponentDefinition("avatarCollision");
            gravityDefinition.AddAttribute<float>("groundLevel");
            ComponentRegistry.Instance.Register(gravityDefinition);
        }

        private void RegisterService()
        {
            string idlContents = File.ReadAllText("avatarCollision.kiara");
            SINFONIServerManager.Instance.SinfoniServer.AmendIDL(idlContents);
            ClientManager.Instance.RegisterClientService("avatarCollision", false, new Dictionary<string, Delegate>
            {
                {"setGroundlevel", (Action<string, float>)SetGroundlevel}
            });
        }

        internal void RegisterToEvents()
        {
            World.Instance.AddedEntity += new EventHandler<EntityEventArgs>(HandleEntityAdded);
        }

        /// <summary>
        /// Sets initial ground level of an entity to its y-attribute. This is as when just added the entity the
        /// client may not yet have determined the ground level (as no ray was cast). To avoid setting the entity
        /// to a wrong level, it is just kept where it is until the first ray is cast
        /// </summary>
        /// <param name="sender">The World</param>
        /// <param name="e">Entity Added event arguments</param>
        private void HandleEntityAdded(Object sender, EntityEventArgs e)
        {
            if (e.Entity.ContainsComponent("avatar"))
            {
                // Initialise entities without gravity
                float initialGroundlevel = ((Vector)e.Entity["location"]["position"].Value).y;
                e.Entity["avatarCollision"]["groundLevel"].Suggest(initialGroundlevel);
            }
        }

        /// <summary>
        /// Checks if position of entity has changed and sets y attribute to ground level if y and ground level
        /// are different
        /// </summary>
        /// <param name="accumulatedTransforms">Accumulated transformation that happened in the service chain</param>
        /// <returns>Accumulated changes with adaptions added by AvatarCollison</returns>
        internal AccumulatedAttributeTransform Transform(AccumulatedAttributeTransform accumulatedTransforms)
        {
            if (!accumulatedTransforms.Entity.ContainsComponent("avatar"))
                return accumulatedTransforms;

            Vector entityPosition = (Vector)accumulatedTransforms.CurrentAttributeValue("location", "position");
            Vector adaptedPosition = new Vector (entityPosition.x,
                (float)accumulatedTransforms.Entity["avatarCollision"]["groundLevel"].Value,
                entityPosition.z);

            accumulatedTransforms.AddAttributeTransformation("location", "position", adaptedPosition);
            return accumulatedTransforms;
        }

        /// <summary>
        /// Service function for clients to set an entity to the ground
        /// </summary>
        /// <param name="entityGuid">Guid of the entity to be changed</param>
        /// <param name="groundLevel">New groundlevel</param>
        public void SetGroundlevel(string entityGuid, float groundLevel)
        {
            var entity = World.Instance.FindEntity(entityGuid);
            entity["avatarCollision"]["groundLevel"].Suggest(groundLevel);
        }
    }
}
