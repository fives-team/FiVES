using System;
using KIARA;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SuperWebSocket;
using Newtonsoft.Json;
using System.Reflection;

namespace WebSocketJSON
{
    public class WSJProtocol : WebSocketSession<WSJProtocol>, IProtocol
    {
        public WSJProtocol()
        {
        }

        public void handleClose(SuperSocket.SocketBase.CloseReason reason)
        {
            foreach (var call in activeCalls)
                call.Value.handleError("Connection closed. Reason: " + reason.ToString());
            activeCalls.Clear();
        }

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
            if (msgType == "call-reply") {
                int callID = Convert.ToInt32(data[1]);
                if (activeCalls.ContainsKey(callID)) {
                    bool success = data[2].ToObject<bool>();
                    JToken result = data[3];
                    if (success)
                        activeCalls[callID].handleSuccess(result);
                    else
                        activeCalls[callID].handleException(result);
                    activeCalls.Remove(callID);
                } else {
                    throw new Error(ErrorCode.CONNECTION_ERROR, 
                                    "Received a response for an unrecognized call id: " + callID);
                }
            } else if (msgType == "call") {
                int callID = data[1].ToObject<int>();
                string methodName = data[2].ToObject<string>();
                if (registeredFunctions.ContainsKey(methodName)) {
                    Delegate nativeMethod = registeredFunctions[methodName];
                    ParameterInfo[] paramInfo = nativeMethod.Method.GetParameters();
                    if (paramInfo.Length != data.Count - 3) {
                        throw new Error(ErrorCode.INVALID_ARGUMENT,
                                        "Incorrect number of arguments for method: " + methodName +
                                        ". Expected: " + paramInfo.Length + ". Got: " + 
                                        (data.Count - 3));
                    }
                    List<object> parameters = new List<object>();
                    for (int i = 0; i < paramInfo.Length; i++)
                        parameters.Add(data[i+3].ToObject(paramInfo[i].GetType()));

                    object returnValue = null;
                    object exception = null;
                    bool success = true;

                    try {
                        returnValue = nativeMethod.DynamicInvoke(parameters.ToArray());
                    } catch (Exception e) {
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
                        else if (nativeMethod.Method.ReturnType != typeof(void))
                            callReplyMessage.Add(returnValue);

                        Send(JsonConvert.SerializeObject(callReplyMessage));
                    }
                } else {
                    throw new Error(ErrorCode.CONNECTION_ERROR, 
                                    "Received a call for an unregistered method: " + methodName);
                }
            } else
                throw new Error(ErrorCode.CONNECTION_ERROR, "Unknown message type: " + msgType);
        }

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

            WSJFuncCall callObj = new WSJFuncCall();

            activeCalls.Add(callID, callObj);
            return callObj;
        }

        private bool isOneWay(string qualifiedMethodName)
        {
            List<string> onewayMethods = new List<string> {
                // Add new one-way calls here
            };

            return onewayMethods.Contains(qualifiedMethodName);
        }

        public void registerHandler(string name, Delegate handler)
        {
            registeredFunctions[name] = handler;
        }

        private int nextCallID = 0;
        private Dictionary<int, WSJFuncCall> activeCalls = new Dictionary<int, WSJFuncCall>();
        private Dictionary<string, Delegate> registeredFunctions = new Dictionary<string, Delegate>();

        #endregion
    }
}

