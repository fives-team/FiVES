// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using FIVES;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using SINFONI;

namespace ClientManagerPlugin
{
    /// <summary>
    /// Implements an Update queue that queues attribute changes of entities and sends those to clients to synchronize with the server state
    /// </summary>
    internal class ClientUpdateQueue
    {
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
        private void FlushUpdateQueue() {
            while (true)
            {
                bool gotLock = false;
                try
                {
                    QueueLock.Enter(ref gotLock);

                    if (UpdateQueue.Count > 0)
                    {
                        InvokeClientCallbacks();
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
        /// Loops over all registered client callbacks and invokes them to inform clients about updates
        /// </summary>
        private void InvokeClientCallbacks()
        {
            lock (CallbackRegistryLock)
            {
                foreach (ClientFunction callback in ClientCallbacks.Values)
                    callback(UpdateQueue);
            }
        }

        /// <summary>
        /// Registers a new client for updates by adding its update callback to the list of callbacks
        /// </summary>
        /// <param name="connection">Client connection</param>
        /// <param name="clientCallback">Callback to be invoked on client to process updates</param>
        internal void RegisterToClientUpdates(Connection connection, ClientFunction clientCallback)
        {
            lock (CallbackRegistryLock)
            {
                if(!ClientCallbacks.ContainsKey(connection))
                    ClientCallbacks.Add(connection, clientCallback);
            }
        }

        /// <summary>
        /// Stops sending updates to a client by removing its callback from the callback registry.
        /// <param name="connection">Client connection</param>
        /// </summary>
        internal void StopClientUpdates(Connection connection)
        {
            lock (CallbackRegistryLock)
            {
                if (ClientCallbacks.ContainsKey(connection))
                    ClientCallbacks.Remove(connection);
            }
        }

        /// <summary>
        /// Starts the update thread that performs the update loop.
        /// </summary>
        private void StartUpdateThread () {
            Task.Factory.StartNew(FlushUpdateQueue, TaskCreationOptions.LongRunning);
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
            newUpdateInfo.entityGuid = entity.Guid.ToString();
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
        private Dictionary<Connection, ClientFunction> ClientCallbacks = new Dictionary<Connection, ClientFunction>();

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

