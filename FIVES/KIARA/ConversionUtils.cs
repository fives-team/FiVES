using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KIARA
{
    internal class ConversionUtils
    {
        public static object CastJObject(object obj, Type destType)
        {
            // Special cases
            if (obj == null)                                    // null
                return null;
            else if (obj is long && destType == typeof(int))    // long -> int
                return Convert.ToInt32((long)obj);
            else if (obj is long && destType == typeof(uint))   // long -> uint
                return Convert.ToUInt32((long)obj);
            else if (obj is double && destType == typeof(float))       // double -> float
                return (float)(double)obj;
            else if (obj is JObject && destType == typeof(Exception))  // construct exceptions
                return new Exception(obj.ToString());
            // General cases
            else if (obj.GetType() == destType)                 // types match
                return obj;
            else if (destType.IsAssignableFrom(obj.GetType()))  // implicit cast will do the job
                return obj;
            else if (obj is JToken) {                          // got JObject, but need actual type
                return ((JToken)obj).ToObject(destType);
            } else                                                // nothing worked - just fail
                throw new Error(ErrorCode.INVALID_TYPE,
                                "Cannot cast " + obj.GetType().Name + " to " + destType.Name);
        }
    }
}

