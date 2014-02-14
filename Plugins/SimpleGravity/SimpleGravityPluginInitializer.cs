﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientManagerPlugin;
using FIVES;

namespace SimpleGravityPlugin
{
    /// <summary>
    /// Simple Gravity introduces an attribute that specifies the current ground level for an avatar as determined by a client.
    /// If the groundlevel is set (may not be the case for non-avatar entities), the entitiy is automatically put to this height.
    /// </summary>
    public class SimpleGravityPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get { return "SimpleGravity"; }
        }

        public List<string> PluginDependencies
        {
            get { return new List<string> { "ClientManager"}; }
        }

        public List<string> ComponentDependencies
        {
            get { return new List<string> {"position"}; }
        }

        public void Initialize()
        {
            RegisterComponents();
            RegisterService();
            RegisterToEvents();
        }

        public void Shutdown()
        {
        }

        /// <summary>
        /// Registers gravity component that carries the attribute for the groundlevel
        /// </summary>
        private void RegisterComponents() {
            ComponentDefinition gravityDefinition = new ComponentDefinition("gravity");
            gravityDefinition.AddAttribute<float>("groundLevel");
            ComponentRegistry.Instance.Register(gravityDefinition);
        }

        private void RegisterService()
        {
            ClientManager.Instance.RegisterClientService("gravity", false, new Dictionary<string, Delegate>
            {
                {"setGroundlevel", (Action<string, float>)SetGroundlevel}
            });
        }

        private void RegisterToEvents()
        {
            World.Instance.AddedEntity += new EventHandler<EntityEventArgs>(HandleEntityAdded);
            foreach (Entity entity in World.Instance)
            {
                entity.ChangedAttribute += new EventHandler<ChangedAttributeEventArgs>(HandleAttributeChanged);
            }
        }

        /// <summary>
        /// Sets initial grouns level of an entity to its y-attribute. This is as when just added the entity the client
        /// may not yet have determined the ground level (as no ray was cast). To avoid setting the entity to a wrong level,
        /// it is just kept where it is until the first ray is cast
        /// </summary>
        /// <param name="sender">The World</param>
        /// <param name="e">Entity Added event arguments</param>
        private void HandleEntityAdded(Object sender, EntityEventArgs e)
        {
            e.Entity["gravity"]["groundLevel"] = e.Entity["position"]["y"]; // Initialise entities without gravity
            e.Entity.ChangedAttribute += new EventHandler<ChangedAttributeEventArgs>(HandleAttributeChanged);
        }

        /// <summary>
        /// Checks if position of entity has changed and sets y attribute to ground level if y and ground level are different
        /// </summary>
        /// <param name="sender">Entity that changed position</param>
        /// <param name="e">The Attribute changed Event Args</param>
        private void HandleAttributeChanged(Object sender, ChangedAttributeEventArgs e)
        {
            if (e.Component.Name == "position")
            {
                Entity entity = (Entity)sender;
                if (entity["position"]["y"] != entity["gravity"]["groundLevel"])
                    entity["position"]["y"] = (float)entity["gravity"]["groundLevel"];
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
            entity["gravity"]["groundLevel"] = groundLevel;
        }
    }
}
