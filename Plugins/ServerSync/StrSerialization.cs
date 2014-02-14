using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ServerSyncPlugin
{
    class Serialization
    {
        public static T DeserializeObject<T>(string serializedObj) where T : ISerializable
        {
            var bytes = Convert.FromBase64String(serializedObj);
            MemoryStream stream = new MemoryStream(bytes);
            BinaryFormatter deserializer = new BinaryFormatter();
            return (T)deserializer.Deserialize(stream);
        }

        public static string SerializeObject<T>(T obj) where T : ISerializable
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter serializer = new BinaryFormatter();
            serializer.Serialize(memStream, obj);
            return Convert.ToBase64String(memStream.ToArray());
        }
    }
}
