using System;
using Newtonsoft.Json.Linq;

namespace KIARA
{
    /// <summary>
    /// Various utils to be used by protocol implementations.
    /// </summary>
    public static class ProtocolUtils
    {
        /// <summary>
        /// Attempts to retrieve a setting with <paramref name="name"/> from the server <paramref name="config"/>. The
        /// value of the setting is then casted into <typeparamref name="T">. If the setting is not present in config
        /// the <paramref name="defValue"/> is returned.
        /// </summary>
        /// <returns>The protocol setting value.</returns>
        /// <param name="config">The server config.</param>
        /// <param name="name">The setting name.</param>
        /// <param name="defValue">The default setting value.</param>
        /// <typeparam name="T">The type to which the setting value should be casted.</typeparam>
        public static T retrieveProtocolSetting<T>(Server config, string name, T defValue) {
            JToken value = config.protocol.SelectToken(name);
            return value != null ? value.ToObject<T>() : defValue;
        }
    }
}

