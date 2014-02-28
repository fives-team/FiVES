using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ServerSyncPlugin
{
    class StringSerializationImpl : IStringSerialization
    {
        public T DeserializeObject<T>(string serializedObj) where T : ISerializable
        {
            var bytes = Convert.FromBase64String(serializedObj);
            MemoryStream stream = new MemoryStream(bytes);
            BinaryFormatter deserializer = new BinaryFormatter();
            return (T)deserializer.Deserialize(stream);
        }

        public string SerializeObject<T>(T obj) where T : ISerializable
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter serializer = new BinaryFormatter();
            serializer.Serialize(memStream, obj);
            return Convert.ToBase64String(memStream.ToArray());
        }
    }
}
