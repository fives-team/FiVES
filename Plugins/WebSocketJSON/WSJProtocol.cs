using System;
using KIARA;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SuperWebSocket;
using Newtonsoft.Json;
using System.Reflection;
using Dynamitey;

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

        public event Close OnClose;

        public void ProcessIDL(string parsedIDL)
        {
            // TODO
        }

        public IFuncCall CallFunc(string name, params object[] args)
        {
            int callID = nextCallID++;
            List<object> callMessage = new List<object>();
            callMessage.Add("call");
            callMessage.Add(callID);
            callMessage.Add(name);

            // Register delegates as callbacks. Pass their registered names instead.
            List<object> convertedArgs = new List<object>();
            List<int> callbacks = new List<int>();
            for (int i = 0; i < args.Length; i++) {
                if (args[i] is Delegate) {
                    var arg = args[i] as Delegate;
                    if (!registeredCallbacks.ContainsKey(arg)) {
                        var callbackUUID = Guid.NewGuid().ToString();
                        registeredCallbacks[arg] = callbackUUID;
                        registeredFunctions[callbackUUID] = arg;
                    }
                    callbacks.Add(i);
                    convertedArgs.Add(registeredCallbacks[arg]);
                } else {
                    convertedArgs.Add(args[i]);
                }
            }

            // Add a list of callback indicies.
            callMessage.Add(callbacks);

            // Add converted arguments.
            callMessage.AddRange(convertedArgs);

            string serializedMessage = JsonConvert.SerializeObject(callMessage);
            Send(serializedMessage);

            if (IsOneWay(name))
                return null;

            IWSJFuncCall callObj = wsjFuncCallFactory.Construct();

            activeCalls.Add(callID, callObj);
            return callObj;
        }

        public void RegisterHandler(string name, Delegate handler)
        {
            if (registeredFunctions.ContainsKey(name))
                throw new HandlerAlreadyRegistered("Handler with " + name + " is already registered.");

            registeredFunctions[name] = handler;
        }

        public void Disconnect()
        {
            Close();
        }

        #endregion

        /// <summary>
        /// Handles the close event. All calls are completely with an error.
        /// </summary>
        /// <param name="reason">The reason for the close event.</param>
        public void HandleClose(SuperSocket.SocketBase.CloseReason reason)
        {
            foreach (var call in activeCalls)
                call.Value.HandleError("Connection closed. Reason: " + reason.ToString());
            activeCalls.Clear();
        }

        private void HandleCallReply(List<JToken> data)
        {
            int callID = Convert.ToInt32(data[1]);
            if (activeCalls.ContainsKey(callID)) {
                bool success = data[2].ToObject<bool>();
                JToken result = data.Count == 4 ? data[3] : new JValue((object)null);
                if (success)
                    activeCalls[callID].HandleSuccess(result);
                else
                    activeCalls[callID].HandleException(result);
                activeCalls.Remove(callID);
            } else {
                // TODO: Report error to another side.
                throw new UnknownCallID("Received a response for an unrecognized call id: " + callID);
            }
        }

        public delegate object GenericWrapper(params object[] arguments);
        private void HandleCall(List<JToken> data)
        {
            int callID = data[1].ToObject<int>();
            string methodName = data[2].ToObject<string>();
            if (registeredFunctions.ContainsKey(methodName)) {
                Delegate nativeMethod = registeredFunctions[methodName];
                ParameterInfo[] paramInfo = nativeMethod.Method.GetParameters();
                List<int> callbacks = data[3].ToObject<List<int>>();
                List<JToken> args = data.GetRange(4, data.Count - 4);

                object[] parameters = new object[args.Count];
                try {
                    if (paramInfo.Length != args.Count)
                        throw new InvalidNumberOfArgs("Incorrect number of arguments for a method.");

                    for (int i = 0; i < args.Count; i++) {
                        if (callbacks.Contains(i)) {
                            if (paramInfo[i].ParameterType == typeof(FuncWrapper)) {
                                var remoteCallbackUUID = args[i].ToObject<string>();
                                parameters[i] = (FuncWrapper)delegate(object[] arguments) {
                                    return CallFunc(remoteCallbackUUID, arguments);
                                };
                            } else if (typeof(Delegate).IsAssignableFrom(paramInfo[i].ParameterType)) {
                                string funcName = args[i].ToObject<string>();
                                Type retType = paramInfo[i].ParameterType.GetMethod("Invoke").ReturnType;

                                var genericWrapper = new GenericWrapper(arguments => {
                                    if (retType == typeof(void)) {
                                        CallFunc(funcName, arguments).Wait();
                                        return null;
                                    } else {
                                        object result = null;
                                        CallFunc(funcName, arguments)
                                          .OnSuccess(delegate(JToken res) { result = res.ToObject(retType); })
                                          .Wait();
                                        return result;
                                    }
                                });

                                parameters[i] = Dynamic.CoerceToDelegate(genericWrapper, paramInfo[i].ParameterType);
                            } else {
                                throw new Exception("Callback parameter is neither a delegate nor a FuncWrapper.");
                            }
                        } else {
                            parameters[i] = args[i].ToObject(paramInfo[i].ParameterType);
                        }
                    }
                } catch {
                    // TODO: Mismatching parameters. Return an error to the remote end.
                    return;
                }


                object returnValue = null;
                object exception = null;
                bool success = true;
                try {
                    returnValue = nativeMethod.DynamicInvoke(parameters);
                } catch (Exception e) {
                    Console.WriteLine(e.ToString());
                    exception = e;
                    success = false;
                }

                if (!IsOneWay(methodName)) {
                    // Send call-reply message.
                    List<object> callReplyMessage = new List<object>();
                    callReplyMessage.Add("call-reply");
                    callReplyMessage.Add(callID);
                    callReplyMessage.Add(success);
                    if (!success)
                        callReplyMessage.Add(exception);
                    else if (nativeMethod.Method.ReturnType != typeof(void))
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
        public void HandleMessage(string message)
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
                HandleCallReply(data);
            else if (msgType == "call")
                HandleCall(data);
            else
                throw new Error(ErrorCode.CONNECTION_ERROR, "Unknown message type: " + msgType);
        }

        private bool IsOneWay(string qualifiedMethodName)
        {
            List<string> onewayMethods = new List<string> {
                // Add new one-way calls here
            };

            return onewayMethods.Contains(qualifiedMethodName);
        }

        protected override void OnSessionClosed(SuperSocket.SocketBase.CloseReason reason)
        {
            base.OnSessionClosed(reason);

            if (OnClose != null)
                OnClose(reason.ToString());
        }

        private int nextCallID = 0;
        private Dictionary<int, IWSJFuncCall> activeCalls = new Dictionary<int, IWSJFuncCall>();
        private Dictionary<string, Delegate> registeredFunctions = new Dictionary<string, Delegate>();
        private Dictionary<Delegate, string> registeredCallbacks = new Dictionary<Delegate, string>();

        #region Testing

        internal WSJProtocol(IWSJFuncCallFactory factory)
        {
            wsjFuncCallFactory = factory;
        }

        private IWSJFuncCallFactory wsjFuncCallFactory;

        #endregion
    }
}

