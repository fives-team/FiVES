using System;
using WebSocket4Net;
using NLog;
using SuperSocket.ClientEngine;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace NativeClient
{
    public class ClientDriver
    {
        public ClientDriver(string serverURI, bool move, bool rotate)
        {
            Logger.Info("Connecting to the server");

            socket = new WebSocket(serverURI);
            socket.Opened += (sender, e) => Logger.Info("Connected to the server");
            socket.Error += (sender, e) => Logger.ErrorException("Connection error", e.Exception);
            socket.Closed += (sender, e) => Logger.Info("Connection closed");
            socket.MessageReceived += (sender, e) => Logger.Debug("Received: {0}", e.Message);
            socket.MessageReceived += HandleMessage;
            socket.Opened += Auth;
            socket.Open();

            Move = move;
            Rotate = rotate;
        }

        #region Behavior

        void Auth(object sender, EventArgs e)
        {
            int callID = Call("kiara.implements", new List<string> { "auth" });
            ExpectReply(callID, Auth2);
        }

        void Auth2(CallReply reply)
        {
            if (!reply.Success)
                Fail("Failed to request auth service: {0}", reply.Message);

            List<bool> retValue = reply.RetValue.ToObject<List<bool>>();
            if (!retValue[0])
                Fail("No auth service.");

            int callID = Call("auth.login", GenerateRandomLogin(), "");
            ExpectReply(callID, LoadWorld);
        }

        void LoadWorld(CallReply reply)
        {
            if (!reply.Success)
                Fail("Failed on authentication: {0}", reply.Message);

            SessionKey = reply.RetValue.ToObject<string>();
            if (new Guid(SessionKey) == Guid.Empty)
                Fail("Incorrect login/password", reply.Message);

            int callID = Call("kiara.implements", new List<string> { "objectsync", "avatar", "editing", "location" });
            ExpectReply(callID, LoadWorld2);
        }

        void LoadWorld2(CallReply reply)
        {
            if (!reply.Success)
                Fail("Failed to request client services: {0}", reply.Message);

            List<bool> retValue = reply.RetValue.ToObject<List<bool>>();
            if (!retValue.TrueForAll(s => s))
                Fail("Client services are not supported", reply.Message);

            string handleNewObject = RegisterFunc((request) => HandleNewObject(request.Args[0]));
            Call("objectsync.notifyAboutNewObjects", new List<int>{1}, SessionKey, handleNewObject);

            string handleMoved = RegisterFunc(HandleMoved);
            Call("location.notifyAboutPositionUpdates", new List<int>{1}, SessionKey, handleMoved);

            string handleRotatated = RegisterFunc(HandleRotatated);
            Call("location.notifyAboutOrientationUpdates", new List<int>{1}, SessionKey, handleRotatated);

            int callID = Call("objectsync.listObjects");
            ExpectReply(callID, AddNewObjects);
        }

        void HandleNewObject(JToken entityInfo)
        {
            EntityInfo info = new EntityInfo {
                Guid = entityInfo["guid"].ToString(),
                Position = entityInfo["position"].ToObject<Vector>(),
                Orientation = entityInfo["orientation"].ToObject<Quat>()
            };

            Logger.Info("New entity: {0}", info.Guid);

            lock (Entities)
                Entities.Add(info);
        }

        void HandleMoved(CallRequest request)
        {
            string guid = request.Args[0].ToString();
            Vector newPos = request.Args[1].ToObject<Vector>();
            Logger.Info("{0} moved to ({1},{2},{3})", guid, newPos.x, newPos.y, newPos.z);
        }

        void HandleRotatated(CallRequest request)
        {
            string guid = request.Args[0].ToString();
            Quat newRot = request.Args[1].ToObject<Quat>();
            Logger.Info("{0} rotated to ({1},{2},{3},{4})", guid, newRot.x, newRot.y, newRot.z, newRot.w);
        }

        void AddNewObjects(CallReply reply)
        {
            if (!reply.Success)
                Fail("Failed to list objects: {0}", reply.Message);

            List<JToken> retValue = reply.RetValue.ToObject<List<JToken>>();
            retValue.ForEach(o => HandleNewObject(o));

            if (Move)
                new Thread(MoveAllEntities).Start();

            if (Rotate)
                new Thread(RotateAllEntities).Start();
        }

        void MoveAllEntities()
        {
            while (true) {
                lock (Entities) {
                    foreach (var info in Entities) {
                        if (info.MovingBackward) {
                            info.Position.x -= 0.1;
                            if (info.Position.x < -10)
                                info.MovingBackward = false;
                        } else {
                            info.Position.x += 0.1;
                            if (info.Position.x > 10)
                                info.MovingBackward = true;
                        }

                        Call("location.updatePosition", SessionKey, info.Guid, info.Position, GetUnixTimestamp());
                    }
                }

                Thread.Sleep(100);
            }
        }

        void RotateAllEntities()
        {
            while (true) {
                lock (Entities) {
                    foreach (var info in Entities) {
                        AxisAngle aa = new AxisAngle();
                        aa.FromQuaternion(info.Orientation);
                        aa.Angle += 0.1;
                        if (aa.Angle > 2 * Math.PI)
                            aa.Angle = 0;
                        info.Orientation = aa.ToQuaternion();

                        Call("location.updateOrientation", SessionKey, info.Guid, info.Orientation, GetUnixTimestamp());
                    }
                }

                Thread.Sleep(100);
            }
        }

        #endregion

        long GetUnixTimestamp()
        {
            TimeSpan span = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
            return (long)span.TotalSeconds;
        }

        CallRequest CreateCallRequest(string messageStr, List<JToken> message)
        {
            return new CallRequest {
                Message = messageStr,
                CallID = message[1].ToObject<int>(),
                FuncName = message[2].ToObject<string>(),
                Callbacks = message[3].ToObject<List<int>>(),
                Args = message.GetRange(4, message.Count - 4)
            };
        }

        CallReply CreateCallReply(string messageStr, List<JToken> message)
        {
            return new CallReply {
                Message = messageStr,
                CallID = message[1].ToObject<int>(),
                Success = message[2].ToObject<bool>(),
                RetValue = message[3]
            };
        }

        void HandleMessage(object sender, MessageReceivedEventArgs e)
        {
            List<JToken> message = JsonConvert.DeserializeObject<List<JToken>>(e.Message);
            string messageType = message[0].ToObject<string>();
            if (messageType == "call-error") {
                Logger.Error("Received error message: {0}", e.Message);
            } else if (messageType == "call") {
                string funcName = message[2].ToObject<string>();

                Action<CallRequest> callback;
                lock (RegisteredFuncs) {
                    if (!RegisteredFuncs.ContainsKey(funcName))
                        Fail("Unexpected func call: {0}", e.Message);
                    callback = RegisteredFuncs[funcName];
                }

                callback(CreateCallRequest(e.Message, message));
            } else if (messageType == "call-reply") {
                int callID = message[1].ToObject<int>();

                Action<CallReply> callback;
                lock (ExpectedReplies) {
                    if (!ExpectedReplies.ContainsKey(callID))
                        return;
                    callback = ExpectedReplies[callID];
                }

                callback(CreateCallReply(e.Message, message));
            }
        }

        void ExpectReply(int callID, Action<CallReply> callback) {
            lock (ExpectedReplies)
                ExpectedReplies.Add(callID, callback);
        }

        string RegisterFunc(Action<CallRequest> callback) {
            string name = Guid.NewGuid().ToString();
            RegisterFunc(name, callback);
            return name;
        }

        void RegisterFunc(string funcName, Action<CallRequest> callback) {
            lock (RegisteredFuncs)
                RegisteredFuncs.Add(funcName, callback);
        }

        int Call(string funcName, params object[] args) {
            return Call(funcName, new List<int>(), args);
        }

        int Call(string funcName, List<int> callbacks, params object[] args) {
            int callID = NextCallID++;
            List<object> message = new List<object>();
            message.Add("call");
            message.Add(callID);
            message.Add(funcName);
            message.Add(callbacks);
            message.AddRange(args);

            var serializedMessage = JsonConvert.SerializeObject(message);
            Logger.Debug("Sending: {0}", serializedMessage);
            socket.Send(serializedMessage);
            return callID;

        }

        void Fail(string format, params object[] args)
        {
            Logger.Fatal(format, args);
            Environment.Exit(-1);
        }

        string GenerateRandomLogin()
        {
            var randomizer = new Random();
            return "user" + randomizer.Next();
        }

        static int NextCallID = 0;

        Dictionary<string, Action<CallRequest>> RegisteredFuncs = new Dictionary<string, Action<CallRequest>>();
        Dictionary<int, Action<CallReply>> ExpectedReplies = new Dictionary<int, Action<CallReply>>();

        // List of all entities.
        List<EntityInfo> Entities = new List<EntityInfo>();

        string SessionKey;

        bool Move;
        bool Rotate;

        static Logger Logger = LogManager.GetCurrentClassLogger();
        static WebSocket socket;
    }
}

