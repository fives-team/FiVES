using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientManagerPlugin;
using FIVES;

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
            get { return new List<string> (); }
        }

        public List<string> ComponentDependencies
        {
            get { return new List<string> {"location"}; }
        }

        public void Initialize()
        {
            RegisterComponents();
            RegisterToEvents();

            PluginManager.Instance.OnAnyPluginInitialized += (o, e) =>
            {
                if (e.pluginName == "ClientManager")
                    RegisterService();
            };
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
            ClientManager.Instance.RegisterClientService("avatarCollision", false, new Dictionary<string, Delegate>
            {
                {"setGroundlevel", (Action<string, float>)SetGroundlevel}
            });
        }

        internal void RegisterToEvents()
        {
            World.Instance.AddedEntity += new EventHandler<EntityEventArgs>(HandleEntityAdded);
            foreach (Entity entity in World.Instance)
            {
                if(entity.ContainsComponent("avatar"))
                {
                    entity.ChangedAttribute += new EventHandler<ChangedAttributeEventArgs>(HandleAttributeChanged);
                }
            }
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
                e.Entity["avatarCollision"]["groundLevel"] =
                    ((Vector)e.Entity["location"]["position"]).y; // Initialise entities without gravity
                e.Entity.ChangedAttribute += new EventHandler<ChangedAttributeEventArgs>(HandleAttributeChanged);
            }
        }

        /// <summary>
        /// Checks if position of entity has changed and sets y attribute to ground level if y and ground level
        /// are different
        /// </summary>
        /// <param name="sender">Entity that changed position</param>
        /// <param name="e">The Attribute changed Event Args</param>
        private void HandleAttributeChanged(Object sender, ChangedAttributeEventArgs e)
        {
            Entity senderEntity = (Entity)sender;
            if (senderEntity.ContainsComponent("avatar") &&  e.Component.Name == "location")
            {
                Entity entity = (Entity)sender;
                Vector entityPosition = (Vector)entity["location"]["position"];

                if (entityPosition.y != (float)entity["avatarCollision"]["groundLevel"])
                {
                    Vector correctedPosition = new Vector(entityPosition.x,
                                                            (float)entity["avatarCollision"]["groundLevel"],
                                                            entityPosition.z);

                    entity["location"]["position"] = correctedPosition;
                }
            }
        }

        /// <summary>
        /// Service function for clients to set an entity to the ground
        /// </summary>
        /// <param name="entityGuid">Guid of the entity to be changed</param>
        /// <param name="groundLevel">New groundlevel</param>
        private void SetGroundlevel(string entityGuid, float groundLevel)
        {
            var entity = World.Instance.FindEntity(entityGuid);
            entity["avatarCollision"]["groundLevel"] = groundLevel;
        }
    }
}
