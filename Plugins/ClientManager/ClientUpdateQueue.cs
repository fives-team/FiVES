using System;
using System.Collections.Generic;
using FIVES;
using Events;
using System.Threading;

namespace ClientManagerPlugin
{
    /// <summary>
    /// Implements an Update queue that queues attribute changes of entities and sends those to clients to synchronize with the server state
    /// </summary>
    internal class ClientUpdateQueue
    {
        internal struct UpdateInfo
        {
            public string entityGuid;
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
        internal ClientUpdateQueue(string clientGuid, Action<List <UpdateInfo>> callback)
        {
            ClientCallback = callback;
            StartUpdateThread();
            RegisterToEntityUpdates();
        }

        /// <summary>
        /// Flushs the update queue. Takes the list of all updates queued for the client and calls the client's callback, passing this list as parameter
        /// </summary>
        private void flushUpdateQueue() {
            while (!ClientDisconnected)
            {
                lock (QueueLock)
                {
                    while (UpdateQueue.Count == 0)
                    {
                        Monitor.Wait(QueueLock);
                    }

                    ClientCallback(UpdateQueue);
                    UpdateQueue.Clear();
                    Monitor.PulseAll(QueueLock);
                }
                Thread.Sleep(10); // Wait shortly to collect a number of atomic attribute updates before sending them for performance reasons
            }
        }

        /// <summary>
        /// Stops the client updates.
        /// </summary>
        internal void StopClientUpdates() {
            ClientDisconnected = true;
        }

        /// <summary>
        /// Starts the update thread that performs the update loop.
        /// </summary>
        private void StartUpdateThread () {
            ThreadPool.QueueUserWorkItem(_ => flushUpdateQueue());
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
            var entityGuids = EntityRegistry.Instance.GetAllGUIDs();
            foreach (Guid guid in entityGuids)
            {
                Entity entity = EntityRegistry.Instance.GetEntity(guid);
                entity.OnAttributeInComponentChanged += new Entity.AttributeInComponentChanged(AddEntityUpdateToQueue);
            }
        }

        /// <summary>
        /// Registers to entity registry's OnEntityAdded to register on attribute updates of entities that are
        /// created after the client has connected
        /// </summary>
        private void RegisterToAddedEntitiesUpdates () {
            EntityRegistry.Instance.OnEntityAdded += (object sender, EntityAddedOrRemovedEventArgs e) => {
                Entity newEntity = EntityRegistry.Instance.GetEntity(e.elementId);
                newEntity.OnAttributeInComponentChanged += new Entity.AttributeInComponentChanged(AddEntityUpdateToQueue);
            };
        }

        /// <summary>
        /// Registers to entity removed event of entity registry to remove all entity updates of the removed entity that are still in the queue
        /// </summary>
        private void RegisterToEntityRemoved () {
            EntityRegistry.Instance.OnEntityRemoved += (object sender, EntityAddedOrRemovedEventArgs e) => {
                RemoveEntityFromQueue(e.elementId);
            };
        }

        /// <summary>
        /// Adds update information about an entity to the update queue after having received an attributeChanged event
        /// </summary>
        /// <param name="sender">Entity that invoked the attribute change event</param>
        /// <param name="e">Event Arguments</param>
        private void AddEntityUpdateToQueue(Object sender, AttributeInComponentEventArgs e) {

            lock (QueueLock)
            {
                while (UpdateQueue.Count > UpdateQueue.Capacity)
                {
                    Monitor.Wait(QueueLock);
                }
                Guid entityGuid = ((Entity)sender).Guid;
                UpdateQueue.Add(createUpdateInfoFromEventArgs(entityGuid, e));
                Monitor.PulseAll(QueueLock);
            }
        }

        /// <summary>
        /// Creates the update info from event arguments.
        /// </summary>
        /// <returns>The update info from event arguments.</returns>
        /// <param name="entityGuid">GUID of the entity triggering the AttributeChanged event</param>
        /// <param name="e">Event arguments</param>
        private UpdateInfo createUpdateInfoFromEventArgs(Guid entityGuid, AttributeInComponentEventArgs e) {
            UpdateInfo newUpdateInfo = new UpdateInfo();
            newUpdateInfo.entityGuid = entityGuid.ToString();
            newUpdateInfo.componentName = e.componentName;
            newUpdateInfo.attributeName = e.attributeName;
            newUpdateInfo.value = e.newValue;
            return newUpdateInfo;
        }

        /// <summary>
        /// Removes all unsent update information of a removed entity from the update queue
        /// </summary>
        /// <param name="entityGuid">Guid of the removed entity/param>
        private void RemoveEntityFromQueue(Guid entityGuid) {
            lock (QueueLock)
            {
                foreach (UpdateInfo entityUpdate in UpdateQueue)
                {
                    if (entityUpdate.entityGuid.Equals(entityGuid))
                    {
                        UpdateQueue.Remove(entityUpdate);
                    }
                }
            }
        }

        /// <summary>
        /// Indicates whether the client for which the queue was created has already disconnected
        /// </summary>
        private volatile bool ClientDisconnected = false;

        /// <summary>
        /// Callback to be called on updates, provided by the client
        /// </summary>
        private Action<List<UpdateInfo>> ClientCallback;

        /// <summary>
        /// Mutex Object for the update queue
        /// </summary>
        private object QueueLock = new object();

        /// <summary>
        /// The update queue.
        /// </summary>
        private List<UpdateInfo> UpdateQueue = new List<UpdateInfo>();
    }
}

