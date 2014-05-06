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
using Newtonsoft.Json;

namespace WebSocketJSON
{
    /// <summary>
    /// Correctly encodes NaN and Inifinity floats into JSON stream. Based on:
    /// http://stackoverflow.com/a/13801482/178315
    /// </summary>
    public class StandardFloatConverter : JsonConverter
    {
        public override bool CanRead
        {
            get
            {
                return false;
            }
        }
        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var val = (double)Convert.ToDouble(value);
            if(Double.IsNaN(val) || Double.IsInfinity(val))
            {
                writer.WriteNull();
                return;
            }

            // Preserve the type, otherwise values such as 3.14f may suddenly be printed as 3.1400001049041748.
            if (value is float)
                writer.WriteValue((float)val);
            else
                writer.WriteValue((double)val);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(double) || objectType == typeof(float);
        }
    }
}

