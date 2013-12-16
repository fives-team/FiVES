using System;
using System.Collections.Generic;
using WebSocket4Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using BinaryProtocol;
using WebSocketJSON;

namespace NativeClient
{
    /// <summary>
    /// A call request parameters. Passed to the callbacks registered for a function called by the server.
    /// </summary>
    class CallRequest
    {
        public CallRequest(string message, List<JToken> parsedMessage)
        {
            Message = message;
            CallID = parsedMessage[1].ToObject<int>();
            FuncName = parsedMessage[2].ToObject<string>();
            Callbacks = parsedMessage[3].ToObject<List<int>>();
            Args = parsedMessage.GetRange(4, parsedMessage.Count - 4);
        }

        public string Message;
        public int CallID;
        public string FuncName;
        public List<int> Callbacks;
        public List<JToken> Args;
    }

    /// <summary>
    /// A call reply parameters. Passed to the callbacks registered for an expected call reply.
    /// </summary>
    class CallReply
    {
        public CallReply(string message, List<JToken> parsedMessage)
        {
            Message = message;
            CallID = parsedMessage[1].ToObject<int>();
            Success = parsedMessage[2].ToObject<bool>();
            RetValue = parsedMessage[3];
        }

        public string Message;
        public int CallID;
        public bool Success;
        public JToken RetValue;
    }

    /// <summary>
    /// Handles the communication with the server.
    /// </summary>
    class Communicator
    {
        public Communicator(string host, int ip)
        {
            socket = new BPSocketAdapter(host, ip);
            socket.Opened += (sender, e) => Logger.Info("Connected to the server");
            socket.Error += (sender, e) => Logger.ErrorException("Connection error", e.Exception);
            socket.Closed += (sender, e) => Logger.Info("Connection closed");
            socket.Message += (sender, e) => Logger.Debug("Received: {0}", e.Message);
            socket.Message += HandleMessage;
            socket.Opened += HandleOpened;
            socket.Closed += HandleClosed;
            socket.Open();

            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        }

        /// <summary>
        /// Occurs when connection is established.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// Occurs when connection is closed.
        /// </summary>
        public event EventHandler Disconnected;

        public bool IsConnected
        {
            get
            {
                return socket.IsConnected;
            }
        }

        /// <summary>
        /// Call the specified funcName with specified args.
        /// </summary>
        /// <param name="funcName">Func name.</param>
        /// <param name="args">Arguments.</param>
        public int Call(string funcName, params object[] args) {
            return Call(funcName, new List<int>(), args);
        }

        /// <summary>
        /// Call the specified funcName with specified callbacks and specified args. The callbacks argument should
        /// contain indicies of arguments that are names of functions registered via RegisterFunc.
        /// </summary>
        /// <param name="funcName">Func name.</param>
        /// <param name="callbacks">Callbacks.</param>
        /// <param name="args">Arguments.</param>
        public int Call(string funcName, List<int> callbacks, params object[] args) {
            int callID = NextCallID++;
            List<object> message = new List<object>();
            message.Add("call");
            message.Add(callID);
            message.Add(funcName);
            message.Add(callbacks);
            message.AddRange(args);

            var serializedMessage = JsonConvert.SerializeObject(message, settings);
            Logger.Debug("Sending: {0}", serializedMessage);
            socket.Send(serializedMessage);
            return callID;

        }

        /// <summary>
        /// Registers the callback for a function. Returns generated function name.
        /// </summary>
        /// <returns>Generated function name.</returns>
        /// <param name="callback">Callback.</param>
        public string RegisterFunc(Action<CallRequest> callback) {
            string name = Guid.NewGuid().ToString();
            RegisterFunc(name, callback);
            return name;
        }

        /// <summary>
        /// Registers the callback for a function with specified name.
        /// </summary>
        /// <param name="funcName">Function name.</param>
        /// <param name="callback">Callback.</param>
        public void RegisterFunc(string funcName, Action<CallRequest> callback) {
            lock (RegisteredFuncs)
                RegisteredFuncs.Add(funcName, callback);
        }

        /// <summary>
        /// Adds a callback to be invoked on a call reply with specified ID.
        /// </summary>
        /// <param name="callID">Call reply ID.</param>
        /// <param name="callback">Callback.</param>
        public void AddReplyHandler(int callID, Action<CallReply> callback) {
            lock (ExpectedReplies)
                ExpectedReplies.Add(callID, callback);
        }

        void HandleOpened(object sender, EventArgs e)
        {
            if (Connected != null)
                Connected(this, new EventArgs());
        }

        void HandleClosed(object sender, EventArgs e)
        {
            if (Disconnected != null)
                Disconnected(this, new EventArgs());
        }

        void HandleCallMessage(string message, List<JToken> parsedMessage)
        {
            string funcName = parsedMessage[2].ToObject<string>();
            Action<CallRequest> callback;

            lock (RegisteredFuncs)
            {
                if (!RegisteredFuncs.ContainsKey(funcName))
                    Logger.Fatal("Unexpected func call: {0}", message);
                callback = RegisteredFuncs[funcName];
            }

            callback(new CallRequest(message, parsedMessage));
        }

        void HandleCallReplyMessage(string message, List<JToken> parsedMessage)
        {
            int callID = parsedMessage[1].ToObject<int>();
            Action<CallReply> callback;

            lock (ExpectedReplies)
            {
                if (!ExpectedReplies.ContainsKey(callID))
                    return;
                callback = ExpectedReplies[callID];
            }

            callback(new CallReply(message, parsedMessage));
        }

        void HandleMessage(object sender, MessageEventArgs e)
        {
            List<JToken> parsedMessage = JsonConvert.DeserializeObject<List<JToken>>(e.Message);
            string messageType = parsedMessage[0].ToObject<string>();

            if (messageType == "call-error")
                Logger.Error("Received error message: {0}", e.Message);
            else if (messageType == "call")
                HandleCallMessage(e.Message, parsedMessage);
            else if (messageType == "call-reply")
                HandleCallReplyMessage(e.Message, parsedMessage);
        }

        /// <summary>
        /// Underlying Web Socket connection.
        /// </summary>
        BPSocketAdapter socket;

        /// <summary>
        /// Registered functions to be invoked on call from another side.
        /// </summary>
        Dictionary<string, Action<CallRequest>> RegisteredFuncs = new Dictionary<string, Action<CallRequest>>();

        /// <summary>
        /// Handlers for expected replies.
        /// </summary>
        Dictionary<int, Action<CallReply>> ExpectedReplies = new Dictionary<int, Action<CallReply>>();

        /// <summary>
        /// Next call ID. Used to generate unique call IDs.
        /// </summary>
        static int NextCallID = 0;

        /// <summary>
        /// Settings for the JSON.NET.
        /// </summary>
        JsonSerializerSettings settings = new JsonSerializerSettings();

        static Logger Logger = LogManager.GetCurrentClassLogger();
    }
}

