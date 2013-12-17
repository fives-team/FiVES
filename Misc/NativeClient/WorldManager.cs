using System;
using System.Collections.Generic;
using NLog;
using Newtonsoft.Json.Linq;
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

            this.communicator = communicator;
            this.SessionKey = sessionKey;

            QueryClientServices();
        }

        /// <summary>
        /// Occurs when the world has loaded initial entities.
        /// </summary>
        public event EventHandler Loaded;

        public void CreateEntity()
        {
            EntityInfo newEntity = new EntityInfo
            {
                Position = new Vector
                {
                    x = random.NextDouble() * 20 - 10,
                    y = random.NextDouble() * 20 - 10,
                    z = random.NextDouble() * 20 - 10
                },
                Orientation = new Quat { x = 0, y = 0, z = 0, w = 1 },
                IsLocallyCreated = true
            };

            int createCall = communicator.Call("editing.createEntityAt", newEntity.Position);
            communicator.AddReplyHandler(createCall, delegate(CallReply reply)
            {
                newEntity.Guid = reply.RetValue.ToString();
                lock (Entities)
                    Entities.Add(newEntity);
                logger.Info("Created entity: {0}", newEntity.Guid);
            });
        }

        /// <summary>
        /// Returns the list of Entities known to the client. Please use locks when accessing this field.
        /// </summary>
        public List<EntityInfo> Entities = new List<EntityInfo>();

        private void QueryClientServices()
        {
            List<string> requiredServices = new List<string> { "objectsync", "avatar", "editing", "location" };
            int callID = communicator.Call("kiara.implements", requiredServices);
            communicator.AddReplyHandler(callID, HandleQueryClientServicesReply);
        }

        private void HandleQueryClientServicesReply(CallReply reply)
        {
            if (!reply.Success)
                logger.Fatal("Failed to request client services: {0}", reply.Message);

            List<bool> retValue = reply.RetValue.ToObject<List<bool>>();
            if (!retValue.TrueForAll(s => s)) {
                logger.Fatal("Required client services are not supported: {0}",
                             String.Join(", ", retValue.FindAll(s => !s)));
            }

            RegisterHandlers();
            RequestAllObjects();
        }

        private void RegisterHandlers()
        {
            string handleNewObject = communicator.RegisterFunc(HandleNewObject);
            communicator.Call("objectsync.notifyAboutNewObjects", new List<int>{1}, SessionKey, handleNewObject);

            string handleUpdate = communicator.RegisterFunc(HandleUpdate);
            communicator.Call("objectsync.notifyAboutObjectUpdates", new List<int> { 1 }, SessionKey, handleUpdate);
        }

        private void HandleNewObject(JToken entityInfo)
        {
            EntityInfo info = new EntityInfo {
                Guid = entityInfo["guid"].ToString()
            };

            if (entityInfo["position"] != null)
                info.Position = entityInfo["position"].ToObject<Vector>();
            else
                info.Position = new Vector { x = 0, y = 0, z = 0 };

            if (entityInfo["orientation"] != null)
                info.Orientation = entityInfo["orientation"].ToObject<Quat>();
            else
                info.Orientation = new Quat { x = 0, y = 0, z = 0, w = 1 };

            logger.Info("New entity: {0}", info.Guid);

            lock (Entities)
                Entities.Add(info);
        }

        private void HandleNewObject(CallRequest request)
        {
            JToken entityInfo = request.Args[0];
            HandleNewObject(entityInfo);
        }

        private void HandleUpdate(CallRequest request)
        {
            List<UpdateInfo> receivedUpdates = request.Args[0].ToObject<List<UpdateInfo>>();
            foreach(UpdateInfo update in receivedUpdates) {
                string entityGuid = update.entityGuid;
                string attribute = update.attributeName;
                string component = update.componentName;
                logger.Info("{0} updated attribute {1} of component {2}", entityGuid, attribute, component);
            }
        }

        private void RequestAllObjects()
        {
            int callID = communicator.Call("objectsync.listObjects");
            communicator.AddReplyHandler(callID, HandleRequestAllObjectReply);
        }

        private void HandleRequestAllObjectReply(CallReply reply)
        {
            if (!reply.Success)
                logger.Fatal("Failed to list objects: {0}", reply.Message);

            List<JToken> retValue = reply.RetValue.ToObject<List<JToken>>();
            retValue.ForEach(HandleNewObject);

            if (Loaded != null)
                Loaded(this, new EventArgs());
        }

        private Communicator communicator;

        static Random random = new Random();
        static Logger logger = LogManager.GetCurrentClassLogger();
    }
}

