using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES_New
{
    public class ComponentEventArgs : EventArgs
    {
        public ComponentEventArgs(IComponent component)
        {
            Component = component;
        }

        public IComponent Component { get; private set; }
    }

    public class ChangedAttributeEventArgs : EventArgs
    {
        public ChangedAttributeEventArgs(IComponent component, string attributeName, object oldValue, object newValue)
        {
            Component = component;
            AttributeName = attributeName;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public IEntity Entity
        {
            get
            {
                return Component.ContainingEntity;
            }
        }

        public IComponent Component { get; private set; }
        public string AttributeName { get; private set; }
        public object OldValue { get; private set; }
        public object NewValue { get; private set; }
    }

    public class EntityEventArgs : EventArgs
    {
        public EntityEventArgs(IEntity entity)
        {
            Entity = entity;
        }

        public IEntity Entity { get; private set; }
    }

    public interface IEntity
    {
        Guid Guid { get; }
        IEnumerable<IComponent> Components { get; }
        IComponent this[string componentName] { get; }

        event EventHandler<ComponentEventArgs> CreatedComponent;
        event EventHandler<ChangedAttributeEventArgs> ChangedAttribute;

        bool ContainsComponent(string componentName);
    }

    public interface IComponent
    {
        string Name { get; }
        IComponentDefinition Definition { get; }
        IEntity ContainingEntity { get; }
        object this[string attributeName] { get; set; }

        event EventHandler<ChangedAttributeEventArgs> ChangedAttribute;
    }

    public interface IComponentRegistry
    {
        IEnumerable<IComponentDefinition> RegisteredComponents { get; }
        IComponentDefinition this[string componentName] { get; }

        void Register(IComponentDefinition definition);
    }

    public interface IComponentDefinition
    {
        string Name { get; }
        IEnumerable<IAttributeDefinition> AttributeDefinitions { get; }
        IAttributeDefinition this[string attributeName] { get; }
    }

    public interface IAttributeDefinition
    {
        string Name { get; }
        Type Type { get; }
        object DefaultValue { get; }
    }

    public interface IEntityCollection : ICollection<IEntity>
    {
        IEntity this[string guid] { get; }
        IEntity this[Guid guid] { get; }

        event EventHandler<EntityEventArgs> AddedEntity;
        event EventHandler<EntityEventArgs> RemovedEntity;
    }

    public interface IECAFactory
    {
        IComponentDefinition CreateComponentDefinition(string name,
            IEnumerable<IAttributeDefinition> attributeDefinitions);

        IAttributeDefinition CreateAttributeDefinition<T>(string name);
        IAttributeDefinition CreateAttributeDefinition<T>(string name, T defaultValue);

        IEntity CreateEntity();
        IEntity CreateEntity(Guid guid);
    }

    public static class ECA
    {
        static public IECAFactory Factory;
        static public IComponentRegistry ComponentRegistry;
        static public IEntityCollection World;
    }

    public class Example
    {
        public void ExampleUsage()
        {
            // Register a new component.
            ECA.ComponentRegistry.Register(ECA.Factory.CreateComponentDefinition("mixup",
                new List<IAttributeDefinition> {
                    ECA.Factory.CreateAttributeDefinition<float>("f", 3.14f),
                    ECA.Factory.CreateAttributeDefinition<int>("y"),
                    ECA.Factory.CreateAttributeDefinition<string>("s", null)
                }
            ));

            // Get an entity by Guid, set an attribute and get an attribute.
            string myEntityGuid = "5c3568cf-60b2-48d9-afc4-dc5cafbee42c";
            ECA.World[myEntityGuid]["mixup"]["y"] = 42;
            float f = (float)ECA.World[myEntityGuid]["mixup"]["f"];

            // Create new entity and add it to the world.
            var newEntity = ECA.Factory.CreateEntity();
            ECA.World.Add(newEntity);

            // Iterate over all attributes in all components in all entities in the world.
            foreach (var entity in ECA.World) {
                foreach (var component in entity.Components) {
                    foreach (var attributeDefinition in component.Definition.AttributeDefinitions) {
                        var attributeName = attributeDefinition.Name;
                        Console.WriteLine(entity.Guid + "." + component.Name + "." +
                                          attributeName + "=" + component[attributeName]);
                    }
                }
            }

        // Iterate over all attributes in all component definitions.
        foreach (var componentDefinition in ECA.ComponentRegistry.RegisteredComponents)
            foreach(var attributeDefinition in componentDefinition.AttributeDefinitions)
                Console.WriteLine(componentDefinition.Name + "." + attributeDefinition.Name);
        }
    }
}
