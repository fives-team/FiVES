using System;
using FIVES;
using System.Collections.Generic;
using KIARA;

namespace DirectCall
{
    /// <summary>
    /// DirectCall protocol implementation.
    /// </summary>
    class DCProtocol : IProtocol
    {
        #region IProtocol implementation

        public void ProcessIDL(string parsedIDL)
        {
            // TODO
        }

        public IFuncCall CallFunc(string name, params object[] args)
        {
            if (!registeredHandlers.ContainsKey(name))
                throw new Error(ErrorCode.INVALID_ARGUMENT, "Handler for the " + name + " is not registered");

            var call = new DCFuncCall();
            try {
                var retValue = registeredHandlers[name].DynamicInvoke(args);
                call.HandleSuccess(retValue);
            } catch (Exception e) {
                call.HandleException(e);
            }
            return call;
        }

        public void RegisterHandler(string name, Delegate handler)
        {
            if (registeredHandlers.ContainsKey(name))
                throw new Error(ErrorCode.INVALID_ARGUMENT, "Handler for the " + name + " is already registered");

            registeredHandlers[name] = handler;
        }

        #endregion

        Dictionary<string, Delegate> registeredHandlers = new Dictionary<string, Delegate>();
    }
}

