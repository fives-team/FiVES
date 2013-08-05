using System;
using KIARA;

namespace DirectCall
{
    public class DCFuncCall : FuncCallBase
    {
        protected override object convertResult(object result, Type type)
        {
            // No need to convert. This is a direct call.
            return result;
        }
    }
}