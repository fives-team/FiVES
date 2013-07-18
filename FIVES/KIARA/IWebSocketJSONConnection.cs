using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KIARA
{
    public delegate void ConnectionMessageDelegate(string data);
    public delegate void ConnectionCloseDelegate();
    public delegate void ConnectionErrorDelegate(string reason);

    public interface IWebSocketJSONConnection
    {
        // Event should be triggered on every new message.
        event ConnectionMessageDelegate OnMessage;

        // Event should be triggered when the connection is closed.
        event ConnectionCloseDelegate OnClose;

        // Event should be triggered when an error is encountered.
        event ConnectionErrorDelegate OnError;

        // Sends a message.
        bool Send(string message);

        // Starts receiving messages (and triggering OnMessage). Previous messages should be cached 
        // until this method is called.
        void Listen();
    }

    public partial class Connection
    {
        public Connection(IWebSocketJSONConnection connection)
        {
            Implementation = new WebSocketJSONConnectionImplementation(connection);
            Implementation.OnClose += delegate(string reason) {
                if (OnClose != null)
                    OnClose(reason);
            };
        }
    }

    #region Private implementation
    internal class WebSocketJSONConnectionImplementation : Connection.IImplementation
    {
        public event Connection.CloseDelegate OnClose;

        public void LoadIDL(string uri)
        {
            // TODO(rryk): Load and parse IDL.
        }

        public WebSocketJSONConnectionImplementation(IWebSocketJSONConnection connection)
        {
            Connection = connection;
            Connection.OnMessage += HandleMessage;
            Connection.OnClose += HandleClose;
            Connection.OnError += HandleError;
            Connection.Listen();
        }

        public FunctionWrapper GenerateFuncWrapper(string qualifiedMethodName, string typeMapping, 
                                                   Dictionary<string, Delegate> defaultHandlers)
        {
            // Validate default handlers.
            foreach (KeyValuePair<string, Delegate> defaultHandler in defaultHandlers)
                FunctionCall.ValidateHandler(defaultHandler.Key, defaultHandler.Value);

            return (FunctionWrapper)delegate(object[] parameters) {
                int callID = NextCallID++;
                List<object> callMessage = new List<object>();
                callMessage.Add("call");
                callMessage.Add(callID);
                callMessage.Add(qualifiedMethodName);
                callMessage.AddRange(parameters);
                string serializedMessage = JsonConvert.SerializeObject(callMessage);
                Connection.Send(serializedMessage);

                if (IsOneWay(qualifiedMethodName))
                    return null;

                FunctionCall callObj = new FunctionCall();
                foreach (KeyValuePair<string, Delegate> defaultHandler in defaultHandlers)
                    callObj.On(defaultHandler.Key, defaultHandler.Value);

                ActiveCalls.Add(callID, callObj);
                return callObj;
            };
        }

        private bool IsOneWay(string qualifiedMethodName)
        {
            List<string> onewayMethods = new List<string> {
                "omp.connectClient.handshake",
                "omp.connectInit.useCircuitCode",
                "omp.connectServer.handshakeReply",
                "omp.objectSync.updateObject",
                "omp.objectSync.deleteObject",
                "omp.objectSync.locationUpdate",
                "omp.movement.updateAvatarLocation",
                "omp.movement.updateAvatarMovement",
                "omp.chatServer.messageFromClient",
                "omp.chatClient.messageFromServer",
                "omp.animationServer.startAnimation"
            };

            return onewayMethods.Contains(qualifiedMethodName);
        }

        private void HandleMessage(string message)
        {
            List<object> data = null;

            // FIXME: Occasionally we receive JSON with some random bytes appended. The reason is
            // unclear, but to be safe we ignore messages that have parsing errors.
            try {
                data = JsonConvert.DeserializeObject<List<object>>(message);
            } catch (JsonException) {
                return;
            }

            string msgType = (string)data[0];
            if (msgType == "call-reply") {
                int callID = Convert.ToInt32(data[1]);
                if (ActiveCalls.ContainsKey(callID)) {
                    bool success = (bool)data[2];
                    object retValOrException = data[3];
                    ActiveCalls[callID].SetResult(success ? "result" : "exception",
                                                  retValOrException);
                    ActiveCalls.Remove(callID);
                } else {
                    throw new Error(ErrorCode.CONNECTION_ERROR, 
                        "Received a response for an unrecognized call id: " + callID);
                }
            } else if (msgType == "call") {
                int callID = Convert.ToInt32(data[1]);
                string methodName = (string)data[2];
                if (RegisteredFunctions.ContainsKey(methodName)) {
                    Delegate nativeMethod = RegisteredFunctions[methodName];
                    ParameterInfo[] paramInfo = nativeMethod.Method.GetParameters();
                    if (paramInfo.Length != data.Count - 3) {
                        throw new Error(ErrorCode.INVALID_ARGUMENT,
                            "Incorrect number of arguments for method: " + methodName +
                            ". Expected: " + paramInfo.Length + ". Got: " + 
                            (data.Count - 3));
                    }
                    List<object> parameters = new List<object>();
                    for (int i = 0; i < paramInfo.Length; i++) {
                        parameters.Add(ConversionUtils.CastJObject(
                            data[i + 3], paramInfo[i].ParameterType));
                    }

                    object returnValue = null;
                    object exception = null;
                    bool success = true;

                    try {
                        returnValue = nativeMethod.DynamicInvoke(parameters.ToArray());
                    } catch (Exception e) {
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

                        Connection.Send(JsonConvert.SerializeObject(callReplyMessage));
                    }
                } else {
                    throw new Error(ErrorCode.CONNECTION_ERROR, 
                        "Received a call for an unregistered method: " + methodName);
                }
            } else
                throw new Error(ErrorCode.CONNECTION_ERROR, "Unknown message type: " + msgType);
        }

        public void HandleClose()
        {
            HandleError("Connection closed.");
        }

        public void HandleError(string reason)
        {
            foreach (KeyValuePair<int, FunctionCall> call in ActiveCalls)
                call.Value.SetResult("error", reason);
            ActiveCalls.Clear();
            if (OnClose != null)
                OnClose(reason);
        }

        public void RegisterFuncImplementation(string qualifiedMethodName, string typeMapping, 
                                               Delegate nativeMethod)
        {
            RegisteredFunctions[qualifiedMethodName] = nativeMethod;
        }

        private IWebSocketJSONConnection Connection;
        private int NextCallID = 0;
        private Dictionary<int, FunctionCall> ActiveCalls = new Dictionary<int, FunctionCall>();
        private Dictionary<string, Delegate> RegisteredFunctions = 
            new Dictionary<string, Delegate>();
    }
    #endregion
}

