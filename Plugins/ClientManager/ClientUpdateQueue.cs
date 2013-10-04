using System;
using System.Collections.Generic;
using FIVES;
using Events;
using System.Threading;

namespace ClientManager
{
    internal class ClientUpdateQueue
    {
        internal struct UpdateInfo
        {
            public string entityGuid;
            public string componentName;
            public string attributeName;
            public int timeStamp; /* not used yet */
            public object value;
        }

        internal ClientUpdateQueue(string clientGuid, Action<List <UpdateInfo>> callback)
        {
            clientCallback = callback;
            StartUpdateThread();
            RegisterToEntityUpdates();
        }

        private void flushUpdateQueue() {
            while (!clientDisconnected)
            {
                lock (queueLock)
                {
                    foreach (KeyValuePair<Guid, List<UpdateInfo>> queuedUpdate in entityUpdates)
                    {
                        clientCallback(queuedUpdate.Value);
                    }
                    entityUpdates.Clear();
                }
                Thread.Sleep(30);
            }
        }

        internal void StopClientUpdates() {
            clientDisconnected = true;
            updateLoopThread.Join();
        }

        private void StartUpdateThread () {
            updateLoopThread = new Thread(flushUpdateQueue);
            updateLoopThread.Start();
        }

        private void RegisterToEntityUpdates() {
            RegisterToExistingEntityUpdates();
            RegisterToAddedEntitiesUpdates();
            RegisterToEntityRemoved();
        }

        private void RegisterToExistingEntityUpdates() {
            var entityGuids = EntityRegistry.Instance.GetAllGUIDs();
            foreach (Guid guid in entityGuids)
            {
                Entity entity = EntityRegistry.Instance.GetEntity(guid);
                entity.OnAttributeInComponentChanged += new Entity.AttributeInComponentChanged(addEntityToQueue);
            }
        }

        private void RegisterToAddedEntitiesUpdates () {
            EntityRegistry.Instance.OnEntityAdded += (object sender, EntityAddedOrRemovedEventArgs e) => {
                Entity newEntity = EntityRegistry.Instance.GetEntity(e.elementId);
                newEntity.OnAttributeInComponentChanged += new Entity.AttributeInComponentChanged(addEntityToQueue);
            };
        }

        private void RegisterToEntityRemoved () {
            EntityRegistry.Instance.OnEntityRemoved += (object sender, EntityAddedOrRemovedEventArgs e) => {
                RemoveEntityFromQueue(e.elementId);
            };
        }

        private void addEntityToQueue(Object sender, AttributeInComponentEventArgs e) {

            lock (queueLock)
            {
                Guid entityGuid = ((Entity)sender).Guid;

                if (!entityUpdates.ContainsKey(entityGuid))
                    initialiseUpdateListForEntity(entityGuid);

                entityUpdates[entityGuid].Add(createUpdateInfoFromEventArgs(entityGuid, e));
            }
        }

        private void initialiseUpdateListForEntity(Guid entityGuid) {
            List<UpdateInfo> updateList = new List<UpdateInfo>();
            entityUpdates.Add(entityGuid, updateList);
        }

        private UpdateInfo createUpdateInfoFromEventArgs(Guid entityGuid, AttributeInComponentEventArgs e) {
            UpdateInfo newUpdateInfo = new UpdateInfo();
            newUpdateInfo.entityGuid = entityGuid.ToString();
            newUpdateInfo.componentName = e.componentName;
            newUpdateInfo.attributeName = e.attributeName;
            newUpdateInfo.value = e.newValue;
            return newUpdateInfo;
        }

        private void RemoveEntityFromQueue(Guid entityGuid) {
            lock (queueLock)
            {
                if (entityUpdates.ContainsKey(entityGuid))
                {
                    entityUpdates.Remove(entityGuid);
                }
            }
        }
        private ClientManager.ClientManagerPlugin.EntityInfo createNewEntityInfo(Guid entityGuid) {
            ClientManager.ClientManagerPlugin.EntityInfo entityInfo = new ClientManager.ClientManagerPlugin.EntityInfo();
            entityInfo.guid = entityGuid.ToString();
            return entityInfo;
        }

        private Thread updateLoopThread;
        private volatile bool clientDisconnected = false;
        private Action<List<UpdateInfo>> clientCallback;
        private object queueLock = new object();
        private IDictionary<Guid, List<UpdateInfo>> entityUpdates = new Dictionary<Guid, List<UpdateInfo>>();
    }
}

