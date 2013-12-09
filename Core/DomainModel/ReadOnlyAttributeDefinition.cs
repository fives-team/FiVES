using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace FIVES
{
    /// <summary>
    /// Represents a read-only attribute definition.
    /// </summary>
    public sealed class ReadOnlyAttributeDefinition
    {
        public ReadOnlyAttributeDefinition(string name, Type type, object defaultValue, Guid guid)
        {
            Guid = guid;
            Name = name;
            Type = type;

            try
            {
                MethodInfo castMethod = GetType().GetMethod("Cast", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo specificCastMethod = castMethod.MakeGenericMethod(type);
                DefaultValue = specificCastMethod.Invoke(null, new object[] { defaultValue });
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException is InvalidCastException)
                {
                    throw new AttributeDefinitionException(
                        "Default value for the attribute can not be cast to its type.");
                }
                else
                {
                    throw e.InnerException;
                }
            }
        }

        /// <summary>
        /// GUID that identifies this attribute definition.
        /// </summary>
        public Guid Guid { get; private set; }

        /// <summary>
        /// Name of the attribute.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Default value for the attribute.
        /// </summary>
        public object DefaultValue { get; private set; }

        /// <summary>
        /// Type of the attribute.
        /// </summary>
        public Type Type { get; private set; }

        private static T Cast<T>(object o)
        {
            return (T)o;
        }

        // Needed by persistence plugin.
        internal ReadOnlyAttributeDefinition() { }
    }
}
