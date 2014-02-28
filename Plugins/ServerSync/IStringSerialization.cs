using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ServerSyncPlugin
{
    public interface IStringSerialization
    {
        T DeserializeObject<T>(string serializedObj) where T : ISerializable;
        string SerializeObject<T>(T obj) where T : ISerializable;
    }
}
