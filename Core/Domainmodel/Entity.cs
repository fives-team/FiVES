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

    public class Entity : DynamicObject
    {
        public Guid Guid { get; private set; }
        private IDictionary<string, Component> components { get; set; }
        public Entity parent { get; set; }
        private List<Entity> children  = new List<Entity> ();
        private ComponentRegistry componentRegistry;

        public delegate void AttributeInComponentChanged(Object sender, AttributeInComponentEventArgs e);
        public event AttributeInComponentChanged OnAttributeInComponentChanged;

        public Entity ()
        {
            componentRegistry = ComponentRegistry.Instance;
            components = new Dictionary<string, Component>();

            // Generate new GUID for this entity.
            Guid = Guid.NewGuid();
        }

        public bool addChildNode(Entity childEntity)
        {
            if (this.children.Contains (childEntity))
                return false;

            if (childEntity.parent != null)
                childEntity.parent.removeChild (childEntity);

            childEntity.parent = this;
            this.children.Add (childEntity);
            return true;
        }

        public bool removeChild(Entity entity)
        {
            if (!this.children.Contains (entity))
                return false;

            entity.parent = null;
            this.children.Remove (entity);
            return true;
        }

        public List<Entity> getAllChildren()
        {
            return children;
        }

        public Entity getFirstChild()
        {
            if(children.Count == 0)
                throw(new EntityHasNoChildrenException("List of children for Entity is empty"));
            return children[0];
        }

        public Entity getLastChild()
        {
            if(children.Count == 0)
                throw(new EntityHasNoChildrenException("List of children for Entity is empty"));
            return children [children.Count -1];
        }

        public bool hasComponent(string name)
        {
            return this.components.ContainsKey (name);
        }

        public Component this[string componentName]
        {
            get
            {
                if (!components.ContainsKey(componentName))
                {
                    if (componentRegistry.isRegistered(componentName))
                        instantiateNewComponent(componentName);
                    else
                        throw new ComponentIsNotDefinedException("Cannot create component '" + componentName + "' as its " +
                            "type is not registered with the ComponentRegistry");
                }
                return this.components[componentName];
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this[binder.Name];
            return true;
        }

        private void instantiateNewComponent(string componentName) {
            components [componentName] = componentRegistry.getComponentInstance (componentName);
            components [componentName].OnAttributeChanged += delegate(object sender, AttributeChangedEventArgs e) {
                if (this.OnAttributeInComponentChanged != null)
                    this.OnAttributeInComponentChanged (this, new AttributeInComponentEventArgs (componentName, e.attributeName, e.value));
            };
        }

        // Used for testing to separate component registry database for different tests.
        internal Entity(ComponentRegistry customComponentRegistry)
        {
            componentRegistry = customComponentRegistry;
            this.components = new Dictionary<string, Component> ();
        }
    }
}

