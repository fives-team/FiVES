using System;
using WebSocket4Net;
using NLog;
using SuperSocket.ClientEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace NativeClient
{
    public class CallFuncAction : IStateAction
    {
        public CallFuncAction(WebSocket socket, string funcName, params object[] arguments)
        {
            lock (NextCallIDLock)
                CallID = NextCallID++;

            FuncName = funcName;
            Arguments = arguments;
            Socket = socket;

            FailureCondition = new ExternalCondition();
            SuccessCondition = new ExternalCondition();
        }

        /// <summary>
        /// This condition will be satisfied when call will return an error, exeception or unexpected value.
        /// </summary>
        /// <value>The failure condition.</value>
        public ExternalCondition FailureCondition { get; private set; }

        /// <summary>
        /// This condition will satisfy when call has returned an expected value.
        /// </summary>
        /// <value>The success condition.</value>
        public ExternalCondition SuccessCondition { get; private set; }

        /// <summary>
        /// Returns cached value. This method should only be called after the call is completed successfully.
        /// </summary>
        /// <returns>The ret value converted to type T.</returns>
        /// <typeparam name="T">Type to which return value should be converted.</typeparam>
        public T GetRetValueAs<T>()
        {
            if (RetValue == null)
                throw new InvalidOperationException("GetRetValueAs may only be invoked after the call is complete");

            return RetValue.ToObject<T>();
        }

        /// <summary>
        /// Sets an expected value. All other values will be considered a failure. If not used, any return value will be
        /// considered as success.
        /// </summary>
        /// <param name="expectedValue">The expected value</param>
        public void SetExpectedValue(object expectedValue) {
            ExpectedValue = expectedValue;
        }

        #region IStateAction implementation

        public void Execute()
        {
            List<object> message = new List<object>();
            message.Add("call");
            message.Add(CallID);
            message.Add(FuncName);
            message.Add(Callbacks);
            message.AddRange(Arguments);

            Socket.MessageReceived += HandleSocketMessage;

            string serializedMessage = JsonConvert.SerializeObject(message);
            Socket.Send(serializedMessage);

            Logger.Debug(serializedMessage);
        }

        #endregion

        void HandleCallReply(bool success, JToken retValue)
        {
            bool exception = !success;
            if (exception) {
                FailureCondition.Satisfy();
            } else {
                if (ExpectedValue == null) {
                    SuccessCondition.Satisfy();
                } else {
                    string expectedValue = JsonConvert.SerializeObject(ExpectedValue);
                    JToken expectedToken = JToken.Parse(expectedValue);

                    if (JToken.DeepEquals(expectedToken, retValue))
                        SuccessCondition.Satisfy();
                    else
                        FailureCondition.Satisfy();

                    RetValue = retValue;
                }
            }
        }

        void HandleSocketMessage(object sender, MessageReceivedEventArgs e)
        {
            try {
                List<JToken> message = JsonConvert.DeserializeObject<List<JToken>>(e.Message);
                string messageType = message[0].ToObject<string>();
                int callID = message[1].ToObject<int>();

                if (messageType == "call")
                    throw new NotImplementedException();
                else if (callID == CallID) {
                    if (messageType == "call-reply") {
                        bool success = message[2].ToObject<bool>();
                        HandleCallReply(success, message[3]);
                    } else if (messageType == "call-error") {
                        FailureCondition.Satisfy();
                    } else {
                        throw new NotSupportedException();
                    }

                    Socket.MessageReceived -= HandleSocketMessage;
                }
            } catch (Exception exception) {
                Logger.WarnException("Failed to parse or process incoming message", exception);
            }
        }

        // Call parameters.
        string FuncName;
        object[] Arguments;
        int CallID;

        // Connection to the server.
        WebSocket Socket;

        // Expected value that is treated as success.
        object ExpectedValue = null;

        // Support for callbacks. Not implemented yet.
        List<int> Callbacks = new List<int>();

        // Cached return value.
        JToken RetValue = null;

        // Call ID generation.
        static object NextCallIDLock = new object();
        static int NextCallID = 0;

        static Logger Logger = LogManager.GetCurrentClassLogger();
    }
}

