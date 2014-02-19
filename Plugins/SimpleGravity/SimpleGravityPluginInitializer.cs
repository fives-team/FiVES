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
            RegisterService();
            RegisterToEvents();
        }

        public void Shutdown()
        {
        }

        private void RegisterComponents() {
            ComponentDefinition gravityDefinition = new ComponentDefinition("gravity");
            gravityDefinition.AddAttribute<float>("drag");
            ComponentRegistry.Instance.Register(gravityDefinition);
        }

        private void RegisterService()
        {
            ClientManager.Instance.RegisterClientService("gravity", false, new Dictionary<string, Delegate>
            {
                {"setDrag", (Action<string, float>)SetDrag}
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

        private void SetDrag(string entityGuid, float dragValue)
        {
            var entity = World.Instance.FindEntity(entityGuid);
            entity["gravity"]["drag"] = dragValue;
        }
    }
}
