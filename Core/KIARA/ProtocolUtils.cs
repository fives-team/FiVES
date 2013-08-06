using System;
using Newtonsoft.Json.Linq;

namespace KIARA
{
    public static class ProtocolUtils
    {
        public static T retrieveProtocolSetting<T>(Server config, string name, T defValue) {
            JToken value = config.protocol.SelectToken(name);
            return value != null ? value.ToObject<T>() : defValue;
        }
    }
}

