using System;
using System.Collections.Generic;
using Events;
using System.Dynamic;

namespace FIVES
{
    public class EntityHasNoChildrenException : Exception
    {
        public EntityHasNoChildrenException() {}
        public EntityHasNoChildrenException(string message) {}
    }

    public class Entity
    {
        public Guid Guid { get; private set; }
        private IDictionary<string, Component> Components { get; set; }
        public Entity Parent { get; set; }
		private List<Entity> Children { get; set; }
        private ComponentRegistry ComponentRegistry;

        public delegate void AttributeInComponentChanged (Object sender, AttributeInComponentEventArgs e);
        public event AttributeInComponentChanged OnAttributeInComponentChanged;

        public delegate void ComponentCreated (Object sender, ComponentCreatedEventArgs e);
        public event ComponentCreated OnComponentCreated;

        public Entity ()
        {
            ComponentRegistry = ComponentRegistry.Instance;
            Components = new Dictionary<string, Component>();

            // Generate new GUID for this entity.
            Guid = Guid.NewGuid();
			Children = new List<Entity> ();
        }

        public bool AddChildNode(Entity childEntity)
        {
            if (this.Children.Contains (childEntity))
                return false;

            if (childEntity.Parent != null)
                childEntity.Parent.RemoveChild (childEntity);

            childEntity.Parent = this;
            this.Children.Add (childEntity);
            return true;
        }

        public bool RemoveChild(Entity entity)
        {
            if (!this.Children.Contains (entity))
                return false;

            entity.Parent = null;
            this.Children.Remove (entity);
            return true;
        }

        public List<Entity> getAllChildren()
        {
            return Children;
        }

        public Entity getFirstChild()
        {
            if(Children.Count == 0)
                throw(new EntityHasNoChildrenException("List of children for Entity is empty"));
            return Children[0];
        }

        public Entity getLastChild()
        {
            if(Children.Count == 0)
                throw(new EntityHasNoChildrenException("List of children for Entity is empty"));
            return Children [Children.Count -1];
        }

        public bool hasComponent(string name)
        {
            return this.Components.ContainsKey (name);
        }

        public Component this[string componentName]
        {
            get
            {
                if (!Components.ContainsKey(componentName))
                {
                    if (ComponentRegistry.IsRegistered(componentName))
                        instantiateNewComponent(componentName);
                    else
                        throw new ComponentIsNotDefinedException("Cannot create component '" + componentName + "' as its " +
                            "type is not registered with the ComponentRegistry");
                }
                return this.Components[componentName];
            }

            internal set {
                Components[componentName] = value;
            }
        }

        private void instantiateNewComponent(string componentName) {
            Component newComponent = ComponentRegistry.GetComponentInstance (componentName);
            newComponent.OnAttributeChanged += delegate(object sender, AttributeChangedEventArgs e) {
                if (this.OnAttributeInComponentChanged != null)
                    this.OnAttributeInComponentChanged (this, new AttributeInComponentEventArgs (componentName, e.attributeName, e.value));
            };
            Components [componentName] = newComponent;
            if (this.OnComponentCreated != null)
                this.OnComponentCreated (this, new ComponentCreatedEventArgs (componentName, newComponent.Guid));
        }

        // Used for testing to separate component registry database for different tests.
        internal Entity(ComponentRegistry customComponentRegistry)
        {
            ComponentRegistry = customComponentRegistry;
            this.Components = new Dictionary<string, Component> ();
        }
    }
}

