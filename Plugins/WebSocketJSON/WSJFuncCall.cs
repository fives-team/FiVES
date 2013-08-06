using System;
using KIARA;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SuperWebSocket;
using System.Threading;
using System.Diagnostics;

namespace WebSocketJSON
{
    #region Testing

    public interface IWSJFuncCall : IFuncCall
    {
        void handleSuccess(JToken retValue);
        void handleException(JToken exception);
        void handleError(string error);
    }

    #endregion

    public class WSJFuncCall : FuncCallBase, IWSJFuncCall
    {
        protected override object convertResult(object result, Type type)
        {
            return ((JToken)result).ToObject(type);
        }

        public void handleSuccess(JToken retValue)
        {
            base.handleSuccess((object)retValue);
        }

        public void handleException(JToken exception)
        {
            base.handleException(new Exception(exception.ToString()));
        }
    }
    
}
