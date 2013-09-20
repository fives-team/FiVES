using System;
using KIARA;

namespace DirectCall
{
    /// <summary>
    /// Call object implementation for DirectCall protocol.
    /// </summary>
    public class DCFuncCall : FuncCallBase
    {
        protected override object ConvertResult(object result, Type type)
        {
            // No need to convert. This is a direct call.
            return result;
        }
    }
}