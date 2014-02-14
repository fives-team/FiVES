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

