using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace NewCorePrototype
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
    ///     public void upgradeMeshResource1to2(IComponent meshResource1, IComponent meshResource2) 
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
        public ComponentDefinition(string name, int version) : base(name, version) { }

        /// <summary>
        /// Adds a new attribute definition to the component definition. Default value for the type is used as default 
        /// value for the attribute.
        /// </summary>
        /// <typeparam name="T">Type of the new attribute.</typeparam>
        /// <param name="name">Name of the new attribute.</param>
        public void AddAttribute<T>(string name)
        {
            AddAttribute<T>(name, default(T));
        }

        /// <summary>
        /// Adds a new attribute definition to the component definition. Specified default value is used.
        /// </summary>
        /// <typeparam name="T">Type of the new attribute.</typeparam>
        /// <param name="name">Name of the new attribute.</param>
        /// /// <param name="defaultValue">Default value of the new attribute.</param>
        public void AddAttribute<T>(string name, object defaultValue)
        {
            if (attributeDefinitions.Find(d => d.Name == name) != null)
                throw new AttributeDefinitionException("Attribute with such name is already defined.");

            attributeDefinitions.Add(new AttributeDefinition(name, typeof(T), defaultValue));
        }
    }
}
