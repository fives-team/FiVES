using System;
using KIARA;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SuperWebSocket;
using System.Threading;
using System.Diagnostics;

namespace WebSocketJSON
{
    public class WSJFuncCall : FuncCallBase
    {
        protected override object convertResult(object result, Type type)
        {
            return ((JToken)result).ToObject(type);
        }

        internal void handleSuccess(JToken retValue)
        {
            base.handleSuccess((object)retValue);
        }

        internal void handleException(JToken exception)
        {
            base.handleException(new Exception(exception.ToString()));
        }

        internal void handleError(JToken error)
        {
            base.handleError(error.ToString());
        }
    }
    
}
