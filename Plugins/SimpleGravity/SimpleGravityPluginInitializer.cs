using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientManagerPlugin;
using FIVES;

namespace SimpleGravityPlugin
{
    public class SimpleGravityPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get { return "SimpleGravity"; }
        }

        public List<string> PluginDependencies
        {
            get { return new List<string> { "ClientManager","Motion"}; }
        }

        public List<string> ComponentDependencies
        {
            get { return new List<string> {"position"}; }
        }

        public void Initialize()
        {
            RegisterComponents();
            RegisterToEvents();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        private void RegisterComponents() {
            ComponentDefinition gravityDefinition = new ComponentDefinition("gravity");
            gravityDefinition.AddAttribute<float>("drag");
            ComponentRegistry.Instance.Register(gravityDefinition);
        }

        private void RegisterToEvents()
        {
            World.Instance.AddedEntity += new EventHandler<EntityEventArgs>(HandleEntityAdded);
            foreach (Entity entity in World.Instance)
            {
                entity.ChangedAttribute += new EventHandler<ChangedAttributeEventArgs>(HandleAttributeChanged);
            }
        }

        private void HandleEntityAdded(Object sender, EntityEventArgs e)
        {
            e.Entity["gravity"]["drag"] = e.Entity["position"]["y"]; // Initialise entities without gravity
            e.Entity.ChangedAttribute += new EventHandler<ChangedAttributeEventArgs>(HandleAttributeChanged);
        }

        private void HandleAttributeChanged(Object sender, ChangedAttributeEventArgs e)
        {
            if (e.Component.Name == "position")
            {
                Entity entity = (Entity)sender;
                if (entity["position"]["y"] != entity["gravity"]["drag"])
                    entity["position"]["y"] = (float)entity["gravity"]["drag"];
            }
        }

    }
}
