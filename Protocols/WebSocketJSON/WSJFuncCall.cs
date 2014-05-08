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
using KIARAPlugin;
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
