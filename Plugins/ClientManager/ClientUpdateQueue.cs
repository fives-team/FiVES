using System;
using System.Collections.Generic;
using FIVES;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ClientManagerPlugin
{
    /// <summary>
    /// Implements an Update queue that queues attribute changes of entities and sends those to clients to synchronize with the server state
    /// </summary>
    internal class ClientUpdateQueue
    {
        internal struct UpdateInfo
        {
            public Guid entityGuid;
            public string componentName;
            public string attributeName;
            //public int timeStamp; /* not used yet */
            public object value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientManager.ClientUpdateQueue"/> class.
        /// </summary>
        /// <param name="clientGuid">GUID (session token) of client for which the queue is created</param>
        /// <param name="callback">Callback provided by the client to be called in each update step</param>
        internal ClientUpdateQueue()
        {            
            StartUpdateThread();
            RegisterToEntityUpdates();
        }

        /// <summary>
        /// Flushs the update queue. Takes the list of all updates queued for the client and calls the client's callback, passing this list as parameter
        /// </summary>
        private void flushUpdateQueue() {
            while (true)
            {
                bool gotLock = false;
                try
                {
                    QueueLock.Enter(ref gotLock);

                    if (UpdateQueue.Count > 0)
                    {
                        ClientCallback(UpdateQueue);
                        UpdateQueue.Clear();
                    }
                }
                finally
                {
                    if (gotLock)
                        QueueLock.Exit();
                }

                // Wait for updates to accumulate (to send them in batches)
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Stops the client updates.
        /// </summary>
        internal void StopClientUpdates(string sessionKey) {
            lock (CallbackRegistryLock)
            {
                if (ClientCallbacks.ContainsKey(sessionKey))
                    ClientCallbacks.Remove(sessionKey);
            }
        }

        /// <summary>
        /// Starts the update thread that performs the update loop.
        /// </summary>
        private void StartUpdateThread () {
            Task.Factory.StartNew(flushUpdateQueue);
        }

        /// <summary>
        /// Registers to Attribute Update events on Entities, as well as Entity Added or Removed of EntityRegistry
        /// </summary>
        private void RegisterToEntityUpdates() {
            RegisterToExistingEntityUpdates();
            RegisterToAddedEntitiesUpdates();
            RegisterToEntityRemoved();
        }

        /// <summary>
        /// Subscribes to Attribute updates of all entities that were already in the scene when the client connected
        /// </summary>
        private void RegisterToExistingEntityUpdates() {
            foreach (Entity entity in World.Instance)
                entity.ChangedAttribute += new EventHandler<ChangedAttributeEventArgs>(AddEntityUpdateToQueue);
        }

        /// <summary>
        /// Registers to entity registry's OnEntityAdded to register on attribute updates of entities that are
        /// created after the client has connected
        /// </summary>
        private void RegisterToAddedEntitiesUpdates () {
            World.Instance.AddedEntity += (object sender, EntityEventArgs e) => {
                e.Entity.ChangedAttribute += new EventHandler<ChangedAttributeEventArgs>(AddEntityUpdateToQueue);
            };
        }

        /// <summary>
        /// Registers to entity removed event of entity registry to remove all entity updates of the removed entity that are still in the queue
        /// </summary>
        private void RegisterToEntityRemoved () {
            World.Instance.RemovedEntity += (object sender, EntityEventArgs e) => {
                RemoveEntityFromQueue(e.Entity);
            };
        }

        /// <summary>
        /// Adds update information about an entity to the update queue after having received an attributeChanged event
        /// </summary>
        /// <param name="sender">Entity that invoked the attribute change event</param>
        /// <param name="e">Event Arguments</param>
        private void AddEntityUpdateToQueue(Object sender, ChangedAttributeEventArgs e) {
            bool gotLock = false;
            try
            {
                QueueLock.Enter(ref gotLock);
                UpdateQueue.Add(CreateUpdateInfoFromEventArgs((Entity)sender, e));
            }
            finally
            {
                if (gotLock)
                    QueueLock.Exit();
            }
        }

        /// <summary>
        /// Creates the update info from event arguments.
        /// </summary>
        /// <returns>The update info from event arguments.</returns>
        /// <param name="entityGuid">GUID of the entity triggering the AttributeChanged event</param>
        /// <param name="e">Event arguments</param>
        private UpdateInfo CreateUpdateInfoFromEventArgs(Entity entity, ChangedAttributeEventArgs e) {
            UpdateInfo newUpdateInfo = new UpdateInfo();
            newUpdateInfo.entityGuid = entity.Guid;
            newUpdateInfo.componentName = e.Component.Name;
            newUpdateInfo.attributeName = e.AttributeName;
            newUpdateInfo.value = e.NewValue;
            return newUpdateInfo;
        }

        /// <summary>
        /// Removes all unsent update information of a removed entity from the update queue
        /// </summary>
        /// <param name="entityGuid">Guid of the removed entity/param>
        private void RemoveEntityFromQueue(Entity entity) {
            bool gotLock = false;
            try
            {
                QueueLock.Enter(ref gotLock);

                foreach (UpdateInfo entityUpdate in UpdateQueue)
                    if (entityUpdate.entityGuid.Equals(entity.Guid))
                        UpdateQueue.Remove(entityUpdate);
            }
            finally
            {
                if (gotLock)
                    QueueLock.Exit();
            }
        }

        /// <summary>
        /// Callback to be called on updates, provided by the client
        /// </summary>
        private Dictionary<string, Action<List<UpdateInfo>>> ClientCallbacks;

        /// <summary>
        /// Mutex Object for the update queue
        /// </summary>
        private SpinLock QueueLock = new SpinLock();

        /// <summary>
        /// Mutex Object for client callback registry
        /// </summary>
        private object CallbackRegistryLock = new object();

        /// <summary>
        /// The update queue.
        /// </summary>
        private List<UpdateInfo> UpdateQueue = new List<UpdateInfo>();
    }
}

