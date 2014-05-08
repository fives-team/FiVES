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
using Newtonsoft.Json.Linq;

namespace KIARAPlugin
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

