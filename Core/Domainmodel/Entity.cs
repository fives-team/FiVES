using System;
using System.Collections.Generic;
using Events;

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

        private IDictionary<string, Component> ComponentHandler
        {
            get
            {
                return Components;
            }
            set
            {
                Components = value;
                InitializeExistingComponents();
            }
        }

        public Entity Parent { get; set; }
        private List<Entity> Children { get; set; }
        private ComponentRegistry ComponentRegistry;

        public delegate void AttributeInComponentChanged (Object sender, AttributeInComponentEventArgs e);

        /// <summary>
        /// Occurs when the value of an attribute in one of the Entitys components has changed.
        /// </summary>
        public event AttributeInComponentChanged OnAttributeInComponentChanged;

        public delegate void ComponentCreated (Object sender, ComponentCreatedEventArgs e);

        /// <summary>
        /// Occurs when lazy creation of one of the Entitys components has finished.
        /// </summary>
        public event ComponentCreated OnComponentCreated;

        public Entity ()
        {
            ComponentRegistry = ComponentRegistry.Instance;
            Components = new Dictionary<string, Component>();

            // Generate new GUID for this entity.
            Guid = Guid.NewGuid();
            Children = new List<Entity> ();
        }

        /// <summary>
        /// Adds a child node to the entity
        /// </summary>
        /// <returns><c>true</c>, if child node was added, <c>false</c> otherwise.</returns>
        /// <param name="childEntity">Child entity.</param>
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

        /// <summary>
        /// Removes a child node.
        /// </summary>
        /// <returns><c>true</c>, if child was removed, <c>false</c> otherwise.</returns>
        /// <param name="entity">Entity to be removed.</param>
        public bool RemoveChild(Entity entity)
        {
            if (!this.Children.Contains (entity))
                return false;

            entity.Parent = null;
            this.Children.Remove (entity);
            return true;
        }

        /// <summary>
        /// returns all children.
        /// </summary>
        /// <returns>All children.</returns>
        public List<Entity> GetAllChildren()
        {
            return Children;
        }

        /// <summary>
        /// Gets the first child.
        /// </summary>
        /// <returns>The first child.</returns>
        public Entity GetFirstChild()
        {
            if(Children.Count == 0)
                throw(new EntityHasNoChildrenException("List of children for Entity is empty"));
            return Children[0];
        }

        /// <summary>
        /// Gets the last child.
        /// </summary>
        /// <returns>The last child.</returns>
        public Entity GetLastChild()
        {
            if(Children.Count == 0)
                throw(new EntityHasNoChildrenException("List of children for Entity is empty"));
            return Children [Children.Count -1];
        }

        /// <summary>
        /// Determines whether this instance has component with the specified name.
        /// </summary>
        /// <returns><c>true</c> if this instance has component with the specified name; otherwise, <c>false</c>.</returns>
        /// <param name="name">Name.</param>
        public bool HasComponent(string name)
        {
            return this.Components.ContainsKey (name);
        }

        /// <summary>
        /// Removes the component.
        /// </summary>
        /// <returns><c>true</c>, if the component was removed, <c>false</c> otherwise.</returns>
        /// <param name="name">Name of the component to be removed.</param>
        public bool RemoveComponent(string name)
        {
            if (HasComponent(name)) {
                Components.Remove(name);
                return true;
            }

            return false;
        }

        public Component this[string componentName]
        {
            get
            {
                if (!Components.ContainsKey(componentName))
                {
                    if (ComponentRegistry.IsRegistered(componentName))
                        InstantiateNewComponent(componentName);
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

        /// <summary>
        /// Lazy instantiation of new component when component of specified name is accessed for the first time.
        /// </summary>
        /// <param name="componentName">Component name.</param>
        private void InstantiateNewComponent(string componentName) {
            Component newComponent = ComponentRegistry.GetComponentInstance (componentName);
            RegisterToComponentEvents(newComponent, componentName);
            Components [componentName] = newComponent;
            if (this.OnComponentCreated != null)
                this.OnComponentCreated (this, new ComponentCreatedEventArgs (componentName, newComponent.Guid));
        }

        private void InitializeExistingComponents()
        {
            foreach (KeyValuePair<string, Component> entry in Components)
            {
                RegisterToComponentEvents(entry.Value, entry.Key);
            }
        }

        private void RegisterToComponentEvents(Component component, string componentName)
        {
            component.OnAttributeChanged += delegate(object sender, AttributeChangedEventArgs e)
            {
                if (this.OnAttributeInComponentChanged != null)
                    this.OnAttributeInComponentChanged(this, new AttributeInComponentEventArgs(componentName, e.AttributeName, e.AttributeGuid, e.NewValue));
            };
        }
        // Used for testing to separate component registry database for different tests.
        internal Entity(ComponentRegistry customComponentRegistry)
        {
            ComponentRegistry = customComponentRegistry;
            this.Components = new Dictionary<string, Component> ();
        }
    }
}

