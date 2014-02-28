using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ServerSyncPlugin
{
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
