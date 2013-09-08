using System;
using KIARA;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SuperWebSocket;
using Newtonsoft.Json;
using System.Reflection;

namespace WebSocketJSON
{
    /// <summary>
    /// An exception that is thrown when an unregistered method is invoked by the remote end.
    /// </summary>
    public class UnregisteredMethod : Exception
    {
        public UnregisteredMethod() : base() { }
        public UnregisteredMethod(string message) : base(message) { }
    }

    /// <summary>
    /// An exception that is thrown when the number of passed arguments does not match the number of arguments in the
    /// registered call handler.
    /// </summary>
    public class InvalidNumberOfArgs : Exception
    {
        public InvalidNumberOfArgs() : base() { }
        public InvalidNumberOfArgs(string message) : base(message) { }
    }

    /// <summary>
    /// An exception that is thrown when an unknown call ID was returned with a call-reply message.
    /// </summary>
    public class UnknownCallID : Exception
    {
        public UnknownCallID() : base() { }
        public UnknownCallID(string message) : base(message) { }
    }

    /// <summary>
    /// An exception that is thrown when a handler for the method have already be registered before. Only one handler
    /// may be registered for one method.
    /// </summary>
    public class HandlerAlreadyRegistered : Exception
    {
        public HandlerAlreadyRegistered() : base() { }
        public HandlerAlreadyRegistered(string message) : base(message) { }
    }

    /// <summary>
    /// WebSocketJSON protocol implementation.
    /// </summary>
    public class WSJProtocol : WebSocketSession<WSJProtocol>, IProtocol
    {
        public WSJProtocol() : this(new WSJFuncCallFactory()) {}

        #region IProtocol implementation

        public void processIDL(string parsedIDL)
        {
            // TODO
        }

        public IFuncCall callFunc(string name, params object[] args)
        {
            int callID = nextCallID++;
            List<object> callMessage = new List<object>();
            callMessage.Add("call");
            callMessage.Add(callID);
            callMessage.Add(name);
            callMessage.AddRange(args);
            string serializedMessage = JsonConvert.SerializeObject(callMessage);
            Send(serializedMessage);

            if (isOneWay(name))
                return null;

            IWSJFuncCall callObj = wsjFuncCallFactory.construct();

            activeCalls.Add(callID, callObj);
            return callObj;
        }

        public void registerHandler(string name, Delegate handler)
        {
            if (registeredFunctions.ContainsKey(name))
                throw new HandlerAlreadyRegistered("Handler with " + name + " is already registered.");

            registeredFunctions[name] = handler;
        }

        #endregion

        /// <summary>
        /// Handles the close event. All calls are completely with an error.
        /// </summary>
        /// <param name="reason">The reason for the close event.</param>
        public void handleClose(SuperSocket.SocketBase.CloseReason reason)
        {
            foreach (var call in activeCalls)
                call.Value.handleError("Connection closed. Reason: " + reason.ToString());
            activeCalls.Clear();
        }

        private void handleCallReply(List<JToken> data)
        {
            int callID = Convert.ToInt32(data[1]);
            if (activeCalls.ContainsKey(callID)) {
                bool success = data[2].ToObject<bool>();
                JToken result = data.Count == 4 ? data[3] : new JValue((object)null);
                if (success)
                    activeCalls[callID].handleSuccess(result);
                else
                    activeCalls[callID].handleException(result);
                activeCalls.Remove(callID);
            } else {
                // TODO: Report error to another side.
                throw new UnknownCallID("Received a response for an unrecognized call id: " + callID);
            }
        }

        private void handleCall(List<JToken> data)
        {
            int callID = data[1].ToObject<int>();
            string methodName = data[2].ToObject<string>();
            if (registeredFunctions.ContainsKey(methodName)) {
                Delegate nativeMethod = registeredFunctions[methodName];
                ParameterInfo[] paramInfo = nativeMethod.Method.GetParameters();
                if (paramInfo.Length != data.Count - 3) {
                    // TODO: Report error to another side.
                    throw new InvalidNumberOfArgs("Incorrect number of arguments for method: " + methodName + 
                                                  ". Expected: " + paramInfo.Length + ". Got: " + (data.Count - 3));
                }

                List<object> parameters = new List<object>();
                for (int i = 0; i < paramInfo.Length; i++)
                    parameters.Add(data[i + 3].ToObject(paramInfo[i].ParameterType));

                object returnValue = null;
                object exception = null;
                bool success = true;
                try {
                    returnValue = nativeMethod.DynamicInvoke(parameters.ToArray());
                }
                catch (Exception e) {
                    exception = e;
                    success = false;
                }
                if (!isOneWay(methodName)) {
                    // Send call-reply message.
                    List<object> callReplyMessage = new List<object>();
                    callReplyMessage.Add("call-reply");
                    callReplyMessage.Add(callID);
                    callReplyMessage.Add(success);
                    if (!success)
                        callReplyMessage.Add(exception);
                    else
                        if (nativeMethod.Method.ReturnType != typeof(void))
                            callReplyMessage.Add(returnValue);
                    Send(JsonConvert.SerializeObject(callReplyMessage));
                }
            } else {
                // TODO: Report error to another side.
                throw new UnregisteredMethod("Received a call for an unregistered method: " + methodName);
            }
        }

        /// <summary>
        /// Handles an incoming message.
        /// </summary>
        /// <param name="message">The incoming message.</param>
        public void handleMessage(string message)
        {
            List<JToken> data = null;

            // FIXME: Occasionally we receive JSON with some random bytes appended. The reason is
            // unclear, but to be safe we ignore messages that have parsing errors.
            try {
                data = JsonConvert.DeserializeObject<List<JToken>>(message);
            } catch (JsonException) {
                return;
            }

            string msgType = data[0].ToObject<string>();
            if (msgType == "call-reply")
                handleCallReply(data);
            else if (msgType == "call")
                handleCall(data);
            else
                throw new Error(ErrorCode.CONNECTION_ERROR, "Unknown message type: " + msgType);
        }

        private bool isOneWay(string qualifiedMethodName)
        {
            List<string> onewayMethods = new List<string> {
                // Add new one-way calls here
            };

            return onewayMethods.Contains(qualifiedMethodName);
        }

        private int nextCallID = 0;
        private Dictionary<int, IWSJFuncCall> activeCalls = new Dictionary<int, IWSJFuncCall>();
        private Dictionary<string, Delegate> registeredFunctions = new Dictionary<string, Delegate>();

        #region Testing

        internal WSJProtocol(IWSJFuncCallFactory factory)
        {
            wsjFuncCallFactory = factory;
        }

        private IWSJFuncCallFactory wsjFuncCallFactory;

        #endregion
    }
}

