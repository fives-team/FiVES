// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ServerSyncPlugin
{
    /// <summary>
    /// Interface to be implemented by the class able to serialization/deserialize objects to/from a string.
    /// </summary>
    public interface IStringSerialization
    {
        /// <summary>
        /// Deserializes an object from a string into a given type.
        /// </summary>
        /// <typeparam name="T">A given type.</typeparam>
        /// <param name="serializedObj">String representation of an object.</param>
        /// <returns>A deserialized object.</returns>
        T DeserializeObject<T>(string serializedObj) where T : ISerializable;

        /// <summary>
        /// Serializes an object of a given type into a string.
        /// </summary>
        /// <typeparam name="T">A given type.</typeparam>
        /// <param name="obj">Object to be serialized.</param>
        /// <returns>String object representation.</returns>
        string SerializeObject<T>(T obj) where T : ISerializable;
    }
}
