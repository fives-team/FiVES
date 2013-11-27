using Dynamitey;
using KIARAPlugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WebSocket4Net;

namespace WebSocketJSON
{
    public delegate object GenericWrapper(params object[] arguments);

    // TODO: rewrite plugin to use processing loop. This will allow to simplify code by removing locks in this class.
    public class WSJConnection : Connection
    {
        public WSJConnection(WSJSession aSession)
            : this()
        {
            isClientConnection = false;
            session = aSession;
            session.Closed += HandleClosed;
            session.MessageReceived += HandleMessage;
        }

        public WSJConnection(IWebSocket aSocket)
            : this()
        {
            isClientConnection = true;
            socket = aSocket;
            socket.Closed += HandleClosed;
            socket.MessageReceived += HandleMessage;
        }

        public override event EventHandler Closed;

        public override void Disconnect()
        {
            if (isClientConnection)
                socket.Close();
            else
                session.Close();
        }

        /// <summary>
        /// Handles an incoming message.
        /// </summary>
        /// <param name="message">The incoming message.</param>
        public void HandleMessage(object sender, MessageReceivedEventArgs e)
        {
            logger.Debug("Received: " + e.Message);

            List<JToken> data = null;
            // FIXME: Occasionally we receive JSON with some random bytes appended. The reason is
            // unclear, but to be safe we ignore messages that have parsing errors.
            try
            {
                data = JsonConvert.DeserializeObject<List<JToken>>(e.Message);
            }
            catch (JsonException)
            {
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

        protected override void ProcessIDL(string parsedIDL)
        {
            // TODO
        }

        protected override IFuncCall CallFunc(string funcName, params object[] args)
        {
            int callID = getValidCallID();

            // Register delegates as callbacks. Pass their registered names instead.
            List<int> callbacks;
            List<object> convertedArgs = convertCallbackArguments(args, out callbacks);
            List<object> callMessage = createCallMessage(callID, funcName, callbacks, convertedArgs);

            string serializedMessage = JsonConvert.SerializeObject(callMessage, settings);
            Send(serializedMessage);

            if (IsOneWay(funcName))
                return null;

            IWSJFuncCall callObj = wsjFuncCallFactory.Construct();
            lock (activeCalls)
                activeCalls.Add(callID, callObj);
            return callObj;
        }

        protected override void RegisterHandler(string funcName, Delegate handler)
        {
            lock (registeredFunctions)
                registeredFunctions[funcName] = handler;
        }

        internal WSJConnection()
        {
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.Converters.Add(new StandardFloatConverter());
        }

        internal void HandleClosed(object sender, EventArgs e)
        {
            IWSJFuncCall[] removedCalls = new IWSJFuncCall[activeCalls.Count];
            lock (activeCalls)
            {
                activeCalls.Values.CopyTo(removedCalls, 0);
                activeCalls.Clear();
            }

            foreach (var call in removedCalls)
                call.HandleError("Connection closed.");

            if (Closed != null)
                Closed(this, e);
        }

        internal virtual void Send(string message)
        {
            logger.Debug("Sending: " + message);
            if (isClientConnection)
                socket.Send(message);
            else
                session.Send(message);
        }

        private void HandleCall(List<JToken> data)
        {
            int callID = data[1].ToObject<int>();
            string methodName = data[2].ToObject<string>();

            Delegate nativeMethod = null;
            lock (registeredFunctions)
            {
                if (registeredFunctions.ContainsKey(methodName))
                    nativeMethod = registeredFunctions[methodName];
            }

            if (nativeMethod != null)
            {
                object[] parameters;
                try
                {
                    var args = data.GetRange(4, data.Count - 4);
                    var callbacks = data[3].ToObject<List<int>>();
                    var paramInfo = new List<ParameterInfo>(nativeMethod.Method.GetParameters());
                    parameters = ConvertParameters(args, callbacks, paramInfo);
                }
                catch (Exception e)
                {
                    SendCallError(callID, e.Message);
                    return;
                }

                object returnValue = null;
                object exception = null;
                bool success = true;
                try
                {
                    returnValue = nativeMethod.DynamicInvoke(parameters);
                }
                catch (Exception e)
                {
                    logger.DebugException("Exception in method handler", e);
                    exception = e;
                    success = false;
                }

                if (!IsOneWay(methodName))
                    SendCallReply(callID, nativeMethod, success, returnValue, exception);
            }
            else
            {
                SendCallError(callID, "Method " + methodName + " is not registered");
                return;
            }
        }

        private object[] ConvertParameters(List<JToken> args, List<int> callbacks, List<ParameterInfo> paramInfo)
        {
            object[] parameters = new object[paramInfo.Count];

            // Special handling for the first parameter if it's of type Connection.
            if (paramInfo.Count > 0 && paramInfo[0].ParameterType.Equals(typeof(Connection)))
            {
                parameters[0] = this;
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

        private void SendCallError(int callID, string reason)
        {
            List<object> errorReplyMessage = new List<object>();
            errorReplyMessage.Add("call-error");
            errorReplyMessage.Add(callID);
            errorReplyMessage.Add(reason);
            Send(JsonConvert.SerializeObject(errorReplyMessage, settings));
        }

        private void HandleCallError(List<JToken> data)
        {
            int callID = data[1].ToObject<int>();
            string reason = data[2].ToObject<string>();

            // Call error with callID = -1 means we've sent something that was not understood by other side or was
            // malformed. This probably means that protocols aren't incompatible or incorrectly implemented on either
            // side.
            if (callID == -1)
                throw new Exception(reason);

            IWSJFuncCall failedCall = null;
            lock (activeCalls)
            {
                if (activeCalls.ContainsKey(callID))
                {
                    failedCall = activeCalls[callID];
                    activeCalls.Remove(callID);
                }
            }

            if (failedCall != null)
                failedCall.HandleError(reason);
            else
                SendCallError(-1, "Invalid callID: " + callID);
        }

        private void HandleCallReply(List<JToken> data)
        {
            int callID = (int)data[1];

            IWSJFuncCall completedCall = null;
            lock (activeCalls)
            {
                if (activeCalls.ContainsKey(callID))
                {
                    completedCall = activeCalls[callID];
                    activeCalls.Remove(callID);
                }
            }

            if (completedCall != null)
            {
                bool success = data[2].ToObject<bool>();
                JToken result = data.Count == 4 ? data[3] : new JValue((object)null);
                if (success)
                    completedCall.HandleSuccess(result);
                else
                    completedCall.HandleException(result);
            }
            else
            {
                SendCallError(-1, "Invalid callID: " + callID);
            }
        }

        private int getValidCallID()
        {
            lock (nextCallIDLock)
            {
                return nextCallID++;
            }
        }

        private bool IsOneWay(string qualifiedMethodName)
        {
            List<string> onewayMethods = new List<string>
            {
                // Add new one-way calls here
            };

            return onewayMethods.Contains(qualifiedMethodName);
        }

        private List<object> createCallMessage(int callID, string name, List<int> callbacks, List<object> convertedArgs)
        {
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

        private List<object> convertCallbackArguments(object[] args, out List<int> callbacks)
        {
            callbacks = createCallbacksFromArguments(args);

            List<object> convertedArgs = new List<object>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is Delegate)
                {
                    var arg = args[i] as Delegate;
                    string callbackGuid = null;
                    lock (registeredCallbacks)
                        callbackGuid = registeredCallbacks[arg];
                    convertedArgs.Add(callbackGuid);
                }
                else
                {
                    convertedArgs.Add(args[i]);
                }
            }
            return convertedArgs;
        }

        private List<int> createCallbacksFromArguments(object[] args)
        {
            List<int> callbacks = new List<int>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is Delegate)
                {
                    var arg = args[i] as Delegate;

                    string callbackGuid = null;
                    lock (registeredCallbacks)
                    {
                        if (!registeredCallbacks.ContainsKey(arg))
                        {
                            callbackGuid = Guid.NewGuid().ToString();
                            registeredCallbacks[arg] = callbackGuid;
                        }
                        else
                        {
                            callbackGuid = registeredCallbacks[arg];
                        }
                    }

                    lock (registeredFunctions)
                        registeredFunctions[callbackGuid] = arg;

                    callbacks.Add(i);
                }
            }
            return callbacks;
        }

        private object nextCallIDLock = new object();
        private int nextCallID = 0;

        private Dictionary<int, IWSJFuncCall> activeCalls = new Dictionary<int, IWSJFuncCall>();
        private Dictionary<string, Delegate> registeredFunctions = new Dictionary<string, Delegate>();
        private Dictionary<Delegate, string> registeredCallbacks = new Dictionary<Delegate, string>();
        private JsonSerializerSettings settings = new JsonSerializerSettings();

        bool isClientConnection;
        private IWebSocket socket;
        private WSJSession session;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        internal IWSJFuncCallFactory wsjFuncCallFactory = new WSJFuncCallFactory();
    }
}
