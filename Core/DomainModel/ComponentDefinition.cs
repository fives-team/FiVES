using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace FIVES
{
    /// <summary>
    /// Represents a modifiable component definition.
    /// 
    /// It can be used to define new components as following:
    /// <example>
    ///     ComponentDefinition meshResource = new ComponentDefinition("meshResource", 1);
    ///     meshResource.AddAttribute<string>("uri", "mesh://default");
    ///     meshResource.AddAttribute<bool>("visible");
    ///     ComponentRegistry.Instance.Register(meshResource);
    /// </example>
    /// 
    /// Attributes may also be upgraded as following:
    /// <example>
    ///     public void upgradeMeshResource1to2(Component meshResource1, Component meshResource2) 
    ///     {
    ///         meshResource2["uri"] = meshResource1["uri"];
    ///         meshResource2["isVisible"] = meshResource1["visible"];
    ///         // we omit scale attribute to keep the default value
    ///     }
    ///     
    ///     // ...
    ///     
    ///     ComponentDefinition meshResource2 = new ComponentDefinition("meshResource", 2);
    ///     meshResource2.AddAttribute<string>("uri", "mesh://default");
    ///     meshResource2.AddAttribute<bool>("isVisible", true);
    ///     meshResource2.AddAttribute<float>("scale", 1.0f);
    ///     ComponentRegistry.Instance.Upgrade(meshResource2, upgradeMeshResource1to2);
    /// </example>
    /// </summary>
    public sealed class ComponentDefinition : ReadOnlyComponentDefinition
    {
        /// <summary>
        /// Constructs an instance of the ComponentDefinition.
        /// </summary>
        /// <param name="name">Name of the component.</param>
        /// <param name="version">Version of the definition.</param>
        public ComponentDefinition(string name, int version = 1) : base(name, version)
        {
        }

        /// <summary>
        /// Adds a new attribute definition to the component definition. Default value for the type is used as default
        /// value for the attribute.
        /// </summary>
        /// <typeparam name="T">Type of the new attribute.</typeparam>
        /// <param name="name">Name of the new attribute.</param>
        public void AddAttribute<T>(string name)
        {
            AddAttribute(name, typeof(T), default(T));
        }

        /// <summary>
        /// Adds a new attribute definition to the component definition. Specified default value is used.
        /// </summary>
        /// <typeparam name="T">Type of the new attribute.</typeparam>
        /// <param name="name">Name of the new attribute.</param>
        /// <param name="defaultValue">Default value of the new attribute.</param>
        public void AddAttribute<T>(string name, object defaultValue)
        {
            AddAttribute(name, typeof(T), defaultValue);
        }

        /// <summary>
        /// Adds a new attribute definition to the component definition with specified default value and type.
        /// </summary>
        /// <param name="name">Name of the new attribute.</param>
        /// <param name="type">Type of the new attribute.</param>
        /// <param name="defaultValue">Default value of the new attribute.</param>
        public void AddAttribute(string name, Type type, object defaultValue)
        {
            if (attributeDefinitions.ContainsKey(name))
                throw new AttributeDefinitionException("Attribute with such name is already defined.");

            attributeDefinitions[name] = new ReadOnlyAttributeDefinition(name, type, defaultValue);
        }

        public override ReadOnlyCollection<ReadOnlyAttributeDefinition> AttributeDefinitions
        {
            get
            {
                return new ReadOnlyCollection<ReadOnlyAttributeDefinition>(attributeDefinitions.Values);
            }
        }

        public override ReadOnlyAttributeDefinition this[string attributeName]
        {
            get
            {
                return attributeDefinitions[attributeName];
            }
        }

        public override bool ContainsAttributeDefinition(string attributeName)
        {
            return attributeDefinitions.ContainsKey(attributeName);
        }

        private Dictionary<string, ReadOnlyAttributeDefinition> attributeDefinitions =
            new Dictionary<string, ReadOnlyAttributeDefinition>();
    }
}
