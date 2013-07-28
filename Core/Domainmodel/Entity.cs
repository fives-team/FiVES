using System;
using System.Collections.Generic;

namespace FIVES
{
    public class EntityHasNoChildrenException : Exception
    {
        public EntityHasNoChildrenException() {}
        public EntityHasNoChildrenException(string message) {}
    }

    public class Entity
    {
		protected virtual Guid Id { get; set; }
		private IDictionary<string, Component> components { get; set; }
        public Entity parent { get; set; }
        private List<Entity> children  = new List<Entity> ();
        private ComponentRegistry componentRegistry;

        public Entity ()
        {
            componentRegistry = ComponentRegistry.Instance;
			this.components = new Dictionary<string, Component>();
        }

        public Component this [string index]
        {
            get {
                if (!components.ContainsKey(index)) {
                    if (componentRegistry.isRegistered(index)) {
                        components[index] = componentRegistry.createComponent(index);
                    } else {
                        throw new ComponentIsNotDefinedException("Cannot create component '" + index + "' as its " +
                                                                 "type is not registered with the ComponentRegistry");
                    }
                }

                return this.components [index];
            }
        }

        public bool addChildNode(Entity childEntity)
        {
            if (this.children.Contains (childEntity))
                return false;

            childEntity.parent = this;
            this.children.Add (childEntity);
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

        // Used for testing to separate component registry database for different tests.
        internal Entity(ComponentRegistry customComponentRegistry)
        {
            componentRegistry = customComponentRegistry;
			this.components = new Dictionary<string, Component> ();
        }
    }
}

