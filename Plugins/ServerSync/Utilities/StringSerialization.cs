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
    /// Static containter of and interface to the IStringSerialization object.
    /// </summary>
    public class StringSerialization
    {
        public static IStringSerialization Instance = new StringSerializationImpl();

        public static T DeserializeObject<T>(string serializedObj) where T : ISerializable
        {
            return Instance.DeserializeObject<T>(serializedObj);
        }

        public static string SerializeObject<T>(T obj) where T : ISerializable
        {
            return Instance.SerializeObject<T>(obj);
        }
    }
}
