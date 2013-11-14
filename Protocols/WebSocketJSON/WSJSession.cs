using System;
using KIARAPlugin;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SuperWebSocket;
using Newtonsoft.Json;
using System.Reflection;
using Dynamitey;
using System.Runtime.InteropServices;

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
    /// WebSocketJSON session implementation. Contains Connection adapter for KIARA.
    /// </summary>
    public class WSJSession : WebSocketSession<WSJSession>
    {
        public WSJSession()
        {
            connectionAdapter = new ConnectionAdapter(this);
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        }

        public void ProcessIDL(string parsedIDL)
        {
            lock (objLock) {
                // TODO
            }
        }

        public IFuncCall CallFunc(string name, params object[] args)
        {
            lock (functionLock)
            {
                int callID = getValidCallID();

                // Register delegates as callbacks. Pass their registered names instead.
                List<int> callbacks = createCallbacksFromArguments(args);
                List<object> convertedArgs = convertCallbackArguments(args);
                List<object> callMessage = createCallMessage(callID, name, callbacks, convertedArgs);

                string serializedMessage = JsonConvert.SerializeObject(callMessage, settings);
                Send(serializedMessage);

                if (IsOneWay(name))
                    return null;

                IWSJFuncCall callObj = wsjFuncCallFactory.Construct();

                lock (objLock)
                    activeCalls.Add(callID, callObj);
                return callObj;
            }
        }

        private int getValidCallID() {
            return nextCallID++;
        }

        private List<object> createCallMessage(int callID, string name, List<int> callbacks, List<object> convertedArgs) {
            List<object> callMessage = new List<object>();
            callMessage.Add("call");
            callMessage.Add(callID);
            callMessage.Add(name);
            // Add a list of callback indicies.
            callMessage.Add(callbacks);
            // Add converted arguments.
            callMessage.AddRange(convertedArgs);

            return callMessage;
        }

        private List<object> convertCallbackArguments(object[] args) {
            List<object> convertedArgs = new List<object>();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is Delegate)
                {
                    var arg = args[i] as Delegate;
                    convertedArgs.Add(registeredCallbacks[arg]);
                }
                else
                {
                    convertedArgs.Add(args[i]);
                }
            }
            return convertedArgs;
        }


        private List<int> createCallbacksFromArguments(object[] args) {
            List<int> callbacks = new List<int>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is Delegate)
                {
                    var arg = args[i] as Delegate;
                    if (!registeredCallbacks.ContainsKey(arg))
                    {
                        registerCallbackFunction(arg);
                    }
                    callbacks.Add(i);
                }
            }
            return callbacks;
        }

        private void registerCallbackFunction(Delegate arg) {
            var callbackUUID = Guid.NewGuid().ToString();
            registeredCallbacks[arg] = callbackUUID;
            registeredFunctions[callbackUUID] = arg;
        }

        public void RegisterHandler(string name, Delegate handler)
        {
            registeredFunctions[name] = handler;
        }

        public void Disconnect()
        {
            Close();
        }

        /// <summary>
        /// Handles the close event. All calls are completely with an error.
        /// </summary>
        /// <param name="reason">The reason for the close event.</param>
        public void HandleClose(SuperSocket.SocketBase.CloseReason reason)
        {
            lock (objLock) {
                foreach (var call in activeCalls)
                    call.Value.HandleError("Connection closed. Reason: " + reason.ToString());
                activeCalls.Clear();
            }
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

                lock(objLock)
                    activeCalls.Remove(callID);
            } else {
                SendCallError(-1, "Invalid callID: " + callID);
            }
        }

        public delegate object GenericWrapper(params object[] arguments);
        private void HandleCall(List<JToken> data)
        {
            int callID = data[1].ToObject<int>();
            string methodName = data[2].ToObject<string>();
            if (registeredFunctions.ContainsKey(methodName)) {
                Delegate nativeMethod = registeredFunctions[methodName];
                object[] parameters;
                try {
                    var args = data.GetRange(4, data.Count - 4);
                    var callbacks = data[3].ToObject<List<int>>();
                    var paramInfo = new List<ParameterInfo>(nativeMethod.Method.GetParameters());
                    parameters = ConvertParameters(args, callbacks, paramInfo);
                } catch (Exception e) {
                    SendCallError(callID, e.Message);
                    return;
                }

                object returnValue = null;
                object exception = null;
                bool success = true;
                try {
                    returnValue = nativeMethod.DynamicInvoke(parameters);
                } catch (Exception e) {
                    Logger.Debug("Exception in method handler: " + e.ToString());
                    exception = e;
                    success = false;
                }

                if (!IsOneWay(methodName))
                    SendCallReply(callID, nativeMethod, success, returnValue, exception);
            } else {
                SendCallError(callID, "Method " + methodName + " is not registered");
                return;
            }
        }

        private object[] ConvertParameters(List<JToken> args, List<int> callbacks, List<ParameterInfo> paramInfo)
        {
            object[] parameters = new object[paramInfo.Count];

            // Special handling for the first parameter if it's of type Connection.
            if (paramInfo.Count > 0 && paramInfo[0].ParameterType.Equals(typeof(Connection))) {
                parameters[0] = connectionAdapter;
                var otherParams = ConvertParameters(args, callbacks, paramInfo.GetRange(1, paramInfo.Count - 1));
                otherParams.CopyTo(parameters, 1);
                return parameters;
            }

            if (paramInfo.Count != args.Count)
            {
                throw new InvalidNumberOfArgs("Incorrect number of arguments for a method. Expected: " +
                                              paramInfo.Count + ". Received: " + args.Count);
            }

            for (int i = 0; i < args.Count; i++)
            {
                if (callbacks.Contains(i))
                {
                    if (paramInfo[i].ParameterType == typeof(FuncWrapper))
                    {
                        parameters[i] = CreateFuncWrapperDelegate(args[i].ToObject<string>());
                    }
                    else if (typeof(Delegate).IsAssignableFrom(paramInfo[i].ParameterType))
                    {
                        parameters[i] = CreateCustomDelegate(args[i].ToObject<string>(), paramInfo[i].ParameterType);
                    }
                    else
                    {
                        throw new Exception("Parameter " + i + " is neither a delegate nor a FuncWrapper. " +
                                            "Cannot pass callback method in its place");
                    }
                }
                else
                {
                    parameters[i] = args[i].ToObject(paramInfo[i].ParameterType);
                }
            }

            return parameters;
        }

        private object CreateCustomDelegate(string funcName, Type delegateType)
        {
            Type retType = delegateType.GetMethod("Invoke").ReturnType;
            var genericWrapper = new GenericWrapper(arguments =>
            {
                if (retType == typeof(void))
                {
                    CallFunc(funcName, arguments);
                    // We do not wait here since SuperWebSocket doesn't process messages while the
                    // current thread is blocked. Waiting would bring the current client's thread
                    // into a deadlock.
                    return null;
                }
                else
                {
                    throw new NotImplementedException("We do not support callbacks with return " +
                        "value yet. This is because we cannot wait for a callback to complete. " +
                        "See more details here: https://redmine.viscenter.de/issues/1406.");

                    //object result = null;
                    //CallFunc(funcName, arguments)
                    //  .OnSuccess(delegate(JToken res) { result = res.ToObject(retType); })
                    //  .Wait();
                    //return result;
                }
            });

            return Dynamic.CoerceToDelegate(genericWrapper, delegateType);
        }

        private FuncWrapper CreateFuncWrapperDelegate(string remoteCallbackUUID)
        {
            return (FuncWrapper)delegate(object[] arguments)
            {
                return CallFunc(remoteCallbackUUID, arguments);
            };
        }

        private void SendCallReply(int callID, Delegate nativeMethod, bool success, object retValue, object exception)
        {
            List<object> callReplyMessage = new List<object>();
            callReplyMessage.Add("call-reply");
            callReplyMessage.Add(callID);
            callReplyMessage.Add(success);
            if (!success)
                callReplyMessage.Add(exception);
            else if (nativeMethod.Method.ReturnType != typeof(void))
                callReplyMessage.Add(retValue);
            Send(JsonConvert.SerializeObject(callReplyMessage, settings));
        }

        void SendCallError(int callID, string reason)
        {
            List<object> errorReplyMessage = new List<object>();
            errorReplyMessage.Add("call-error");
            errorReplyMessage.Add(callID);
            errorReplyMessage.Add(reason);
            Send(JsonConvert.SerializeObject(errorReplyMessage, settings));
        }

        void HandleCallError(List<JToken> data)
        {
            int callID = data[1].ToObject<int>();
            string reason = data[2].ToObject<string>();

            // Call error with callID=-1 means something we've sent something that was not understood by other side or
            // was malformed. This probably means that protocols aren't incompatible or incorrectly implemented on
            // either side.
            if (callID == -1)
                throw new Exception(reason);

            if (activeCalls.ContainsKey(callID)) {
                activeCalls[callID].HandleError(reason);
            } else {
                SendCallError(-1, "Invalid callID: " + callID);
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
            else if (msgType == "call-error")
                HandleCallError(data);
            else if (msgType == "call")
                HandleCall(data);
            else
                SendCallError(-1, "Unknown message type: " + msgType);
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
            connectionAdapter.HandleClosed(reason.ToString());
        }

        private int nextCallID = 0;
        private object objLock = new object();  // needed because multiple threads may decide to send something
        private object functionLock = new object();
        private Dictionary<int, IWSJFuncCall> activeCalls = new Dictionary<int, IWSJFuncCall>();
        private Dictionary<string, Delegate> registeredFunctions = new Dictionary<string, Delegate>();
        private Dictionary<Delegate, string> registeredCallbacks = new Dictionary<Delegate, string>();
        private JsonSerializerSettings settings = new JsonSerializerSettings();

        internal class ConnectionAdapter : Connection
        {
            public ConnectionAdapter(WSJSession aSession)
            {
                session = aSession;
            }

            public override event EventHandler<ClosedEventArgs> Closed;

            public override void Disconnect()
            {
                session.Disconnect();
            }

            protected override void ProcessIDL(string parsedIDL)
            {
                session.ProcessIDL(parsedIDL);
            }

            protected override IFuncCall CallFunc(string funcName, params object[] args)
            {
                return session.CallFunc(funcName, args);
            }

            protected override void RegisterHandler(string funcName, Delegate handler)
            {
                session.RegisterHandler(funcName, handler);
            }

            public void HandleClosed(string reason)
            {
                if (Closed != null)
                    Closed(session, new ClosedEventArgs(reason));
            }

            private WSJSession session;
        }

        internal ConnectionAdapter connectionAdapter;
        internal IWSJFuncCallFactory wsjFuncCallFactory = new WSJFuncCallFactory();
    }
}

