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
        void HandleSuccess(JToken retValue);
        void HandleException(JToken exception);
        void HandleError(string error);
    }

    #endregion

    /// <summary>
    /// Call object implementation for WebSocketJSON protocol.
    /// </summary>
    public class WSJFuncCall : FuncCallBase, IWSJFuncCall
    {
        protected override object ConvertResult(object result, Type type)
        {
            if (type == typeof(JToken))
                return result;
            return ((JToken)result).ToObject(type);
        }

        /// <summary>
        /// Handles the successful completion of the call.
        /// </summary>
        /// <param name="retValue">Ret value returned from the call.</param>
        public void HandleSuccess(JToken retValue)
        {
            base.HandleSuccess((object)retValue);
        }

        /// <summary>
        /// Handles the exception thrown from the call.
        /// </summary>
        /// <param name="exception">Exception that was thrown.</param>
        public void HandleException(JToken exception)
        {
            base.HandleException(new Exception(exception.ToString()));
        }
    }
    
}
