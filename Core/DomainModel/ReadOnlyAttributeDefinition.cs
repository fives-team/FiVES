// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.

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
