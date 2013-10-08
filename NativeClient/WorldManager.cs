using System;
using System.Collections.Generic;
using NLog;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Threading;

namespace NativeClient
{
    class WorldManager
    {
        /// <summary>
        /// Session key used to associated the world with authenticated client.
        /// </summary>
        /// <value>The session key.</value>
        public string SessionKey { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeClient.WorldManager"/> class.
        /// </summary>
        /// <param name="communicator">Connected communicator.</param>
        /// <param name="sessionKey">Client session key.</param>
        public WorldManager(Communicator communicator, string sessionKey)
        {
            if (!communicator.IsConnected)
                throw new ArgumentException("Communicator must be connected when passed to WorldManager constructor.");

            Communicator = communicator;
            SessionKey = sessionKey;

            QueryClientServices();
        }

        /// <summary>
        /// Occurs when the world has loaded initial entities.
        /// </summary>
        public event EventHandler Loaded;

        public void StartMovingAllEntities()
        {
            new Thread(MoveAllEntities).Start();
        }

        public void StartRotatingAllEntities()
        {
            new Thread(RotateAllEntities).Start();
        }

        void MoveEntity(EntityInfo info)
        {
            if (info.MovingBackward)
            {
                info.Position.x -= 0.1;
                if (info.Position.x < -10)
                    info.MovingBackward = false;
            }
            else
            {
                info.Position.x += 0.1;
                if (info.Position.x > 10)
                    info.MovingBackward = true;
            }

            Communicator.Call("location.updatePosition", SessionKey, info.Guid, info.Position, UnixTimestamp.Now);
        }

        void MoveAllEntities()
        {
            while (true) {
                lock (Entities) {
                    foreach (var info in Entities)
                        MoveEntity(info);
                }

                Thread.Sleep(100);
            }
        }

        void RotateEntity(EntityInfo info)
        {
            AxisAngle aa = new AxisAngle();
            aa.FromQuaternion(info.Orientation);
            aa.Angle += 0.1;

            if (aa.Angle > 2 * Math.PI)
                aa.Angle = 0;

            info.Orientation = aa.ToQuaternion();

            Communicator.Call("location.updateOrientation", SessionKey, info.Guid, info.Orientation, UnixTimestamp.Now);
        }

        void RotateAllEntities()
        {
            while (true) {
                lock (Entities) {
                    foreach (var info in Entities)
                        RotateEntity(info);
                }

                Thread.Sleep(100);
            }
        }

        void QueryClientServices()
        {
            List<string> requiredServices = new List<string> { "objectsync", "avatar", "editing", "location" };
            int callID = Communicator.Call("kiara.implements", requiredServices);
            Communicator.AddReplyHandler(callID, HandleQueryClientServicesReply);
        }

        void HandleQueryClientServicesReply(CallReply reply)
        {
            if (!reply.Success)
                Logger.Fatal("Failed to request client services: {0}", reply.Message);

            List<bool> retValue = reply.RetValue.ToObject<List<bool>>();
            if (!retValue.TrueForAll(s => s)) {
                Logger.Fatal("Required client services are not supported: {0}",
                             String.Join(", ", retValue.FindAll(s => !s)));
            }

            RegisterHandlers();
            RequestAllObjects();
        }

        void RegisterHandlers()
        {
            string handleNewObject = Communicator.RegisterFunc(HandleNewObject);
            Communicator.Call("objectsync.notifyAboutNewObjects", new List<int>{1}, SessionKey, handleNewObject);

            string handleUpdate = Communicator.RegisterFunc(HandleUpdate);
            Communicator.Call("objectsync.notifyAboutObjectUpdates", new List<int> { 1 }, SessionKey, handleUpdate);
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

        void HandleNewObject(CallRequest request)
        {
            JToken entityInfo = request.Args[0];
            HandleNewObject(entityInfo);
        }

        void HandleUpdate(CallRequest request)
        {
            List<UpdateInfo> receivedUpdates = request.Args[0].ToObject<List<UpdateInfo>>();
            foreach(UpdateInfo update in receivedUpdates) {
                string entityGuid = update.entityGuid;
                string attribute = update.attributeName;
                string component = update.componentName;
                Logger.Info("{0} updated attribute {1} of component {2}", entityGuid, attribute, component);
            }
        }

        void RequestAllObjects()
        {
            int callID = Communicator.Call("objectsync.listObjects");
            Communicator.AddReplyHandler(callID, HandleRequestAllObjectReply);
        }

        void HandleRequestAllObjectReply(CallReply reply)
        {
            if (!reply.Success)
                Logger.Fatal("Failed to list objects: {0}", reply.Message);

            List<JToken> retValue = reply.RetValue.ToObject<List<JToken>>();
            retValue.ForEach(HandleNewObject);

            if (Loaded != null)
                Loaded(this, new EventArgs());
        }

        Communicator Communicator;

        List<EntityInfo> Entities = new List<EntityInfo>();

        static Logger Logger = LogManager.GetCurrentClassLogger();
    }
}

