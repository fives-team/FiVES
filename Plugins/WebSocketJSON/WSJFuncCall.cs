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

    /// <summary>
    /// Call object implementation for WebSocketJSON protocol.
    /// </summary>
    public class WSJFuncCall : FuncCallBase, IWSJFuncCall
    {
        protected override object convertResult(object result, Type type)
        {
            if (type == typeof(JToken))
                return result;
            return ((JToken)result).ToObject(type);
        }

        /// <summary>
        /// Handles the successful completion of the call.
        /// </summary>
        /// <param name="retValue">Ret value returned from the call.</param>
        public void handleSuccess(JToken retValue)
        {
            base.handleSuccess((object)retValue);
        }

        /// <summary>
        /// Handles the exception thrown from the call.
        /// </summary>
        /// <param name="exception">Exception that was thrown.</param>
        public void handleException(JToken exception)
        {
            base.handleException(new Exception(exception.ToString()));
        }
    }
    
}
