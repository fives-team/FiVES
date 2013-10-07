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
            ClientCallback = callback;
            StartUpdateThread();
            RegisterToEntityUpdates();
        }

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
            }
        }

        internal void StopClientUpdates() {
            ClientDisconnected = true;
        }

        private void StartUpdateThread () {
            ThreadPool.QueueUserWorkItem(_ => flushUpdateQueue());
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
                entity.OnAttributeInComponentChanged += new Entity.AttributeInComponentChanged(AddEntityUpdateToQueue);
            }
        }

        private void RegisterToAddedEntitiesUpdates () {
            EntityRegistry.Instance.OnEntityAdded += (object sender, EntityAddedOrRemovedEventArgs e) => {
                Entity newEntity = EntityRegistry.Instance.GetEntity(e.elementId);
                newEntity.OnAttributeInComponentChanged += new Entity.AttributeInComponentChanged(AddEntityUpdateToQueue);
            };
        }

        private void RegisterToEntityRemoved () {
            EntityRegistry.Instance.OnEntityRemoved += (object sender, EntityAddedOrRemovedEventArgs e) => {
                RemoveEntityFromQueue(e.elementId);
            };
        }

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

        private UpdateInfo createUpdateInfoFromEventArgs(Guid entityGuid, AttributeInComponentEventArgs e) {
            UpdateInfo newUpdateInfo = new UpdateInfo();
            newUpdateInfo.entityGuid = entityGuid.ToString();
            newUpdateInfo.componentName = e.componentName;
            newUpdateInfo.attributeName = e.attributeName;
            newUpdateInfo.value = e.newValue;
            return newUpdateInfo;
        }

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
        private ClientManager.ClientManagerPlugin.EntityInfo createNewEntityInfo(Guid entityGuid) {
            ClientManager.ClientManagerPlugin.EntityInfo entityInfo = new ClientManager.ClientManagerPlugin.EntityInfo();
            entityInfo.guid = entityGuid.ToString();
            return entityInfo;
        }

        private volatile bool ClientDisconnected = false;
        private Action<List<UpdateInfo>> ClientCallback;
        private object QueueLock = new object();
        private List<UpdateInfo> UpdateQueue = new List<UpdateInfo>();
    }
}

