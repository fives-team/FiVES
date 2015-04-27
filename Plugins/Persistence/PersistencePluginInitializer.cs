// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using FIVES;
using NHibernate.Cfg;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using System.Collections.Generic;
using System.Diagnostics;
using Iesi.Collections.Generic;
using System.Threading;
using System.Data;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using NLog;


namespace PersistencePlugin
{
    public class PersistencePlugin: IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string Name
        {
            get
            {
                return "Persistence";
            }
        }

        public List<string> PluginDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        public List<string> ComponentDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Initializes the plugin. This method will be called by the plugin manager when all dependency plugins have
        /// been loaded.
        /// </summary>
        public void Initialize()
        {
            InitializeNHibernate ();
            World.Instance.AddedEntity += OnEntityAdded;
            World.Instance.RemovedEntity += OnEntityRemoved;
            //ComponentRegistry.Instance.UpgradedComponent += OnComponentUpgraded;
            InitializePersistedCollections();
            ThreadPool.QueueUserWorkItem(_ => PersistChangedEntities());
        }

        public void Shutdown()
        {
        }

        /// <summary>
        /// Initializes NHibernate. Adds the mappings based on the assembly, initializes the session factory and opens
        /// a long-running global session
        /// </summary>
        private void InitializeNHibernate() {
            NHibernateConfiguration.Configure ();
            NHibernateConfiguration.AddAssembly (typeof(Entity).Assembly);
            new SchemaUpdate (NHibernateConfiguration).Execute(false, true);
            SessionFactory = NHibernateConfiguration.BuildSessionFactory ();
            GlobalSession = SessionFactory.OpenSession ();
        }

        /// <summary>
        /// Initializes the state of the system as stored in the database by retrieving the stored component registry and
        /// the stored entities
        /// </summary>
        private void InitializePersistedCollections() {
            RetrieveEntitiesFromDatabase ();
        }

        #endregion

        #region Event Handlers
        /// <summary>
        /// Event Handler for EntityAdded event fired by the EntityRegistry. A new entity will be persisted to the database,
        /// unless the entity to be added was just queried from the database (e.g. upon initialization)
        /// </summary>
        /// <param name="sender">Sender of the event (the EntityRegistry)</param>
        /// <param name="e">Event arguments</param>
        internal void OnEntityAdded(Object sender, EntityEventArgs e)
        {
            Entity addedEntity = e.Entity;
            // Only persist entities if they are not added during intialization on Startup
            if (!EntitiesToInitialize.Contains (addedEntity.Guid)) {
                AddEntityToPersisted (addedEntity);
            } else {
                EntitiesToInitialize.Remove(addedEntity.Guid);                
            }
            addedEntity.ChangedAttribute += new EventHandler<ChangedAttributeEventArgs>(OnAttributeChanged);
            addedEntity.CreatedComponent += new EventHandler<ComponentEventArgs>(HandleComponentCreated);
        }

        /// <summary>
        /// Event Handler for EntityRemoved event fired by the entity registry. Once an entity is removed from the registry,
        /// its entry is also deleted from the database (including cascaded deletion of its component instances)
        /// </summary>
        /// <param name="sender">Sender of the event (the EntityRegistry)</param>
        /// <param name="e">Event Arguments</param>
        internal void OnEntityRemoved(Object sender, EntityEventArgs e) {
            RemoveEntityFromDatabase (e.Entity);
        }

        /// <summary>
        /// Event Handler for AttributeInComponentChanged on Entity. When fired, the value update is queued to be persisted on the next persistence
        /// save to the database
        /// </summary>
        /// <param name="sender">Sender of the event (the Entity)</param>
        /// <param name="e">Event arguments</param>
        internal void OnAttributeChanged(Object sender, ChangedAttributeEventArgs e) {
            Guid changedAttributeGuid = e.Component.Definition[e.AttributeName].Guid;
            AddAttributeToPersisted (e.Component.Guid, e.AttributeName, e.NewValue);
        }

        /// <summary>
        /// When a Component was upgraded to a new version, the entity that is informing about the upgrade is entirely (cascaded) persisted to the database
        /// </summary>
        /// <param name="sender">Sender of the event (the Entity)</param>
        /// <param name="e">Event arguments</param>
        internal void OnComponentUpgraded(Object sender, ComponentEventArgs e) {

            // TODO: change cascading persistence of entity, but only persist component and take care to persist mapping to entity as well
            //AddEntityToPersisted (e.Component.Parent);
        }

        internal void HandleComponentCreated(Object sender, ComponentEventArgs e)
        {
            AddEntityToPersisted((Entity)sender);
        }

        #endregion

        #region database synchronisation

        /// <summary>
        /// Thread worker function that performs the persisting steps for queued entities and attributes
        /// </summary>
        private void PersistChangedEntities() {
            while (true)
            {
                lock (entityQueueLock)
                {
                    if (EntitiesToPersist.Count > 0 )
                    {
                        CommitCurrentEntityUpdates();
                        EntitiesToPersist.Clear();
                    }
                }
                lock (attributeQueueLock)
                {
                    if (ComponentAttributesToPersist.Count > 0)
                    {
                        CommitCurrentAttributeUpdates();
                        ComponentAttributesToPersist.Clear();
                    }
                }
                Thread.Sleep(5);
            }
        }

        /// <summary>
        /// Flushes the queue of stored entity updates and commits the changes to the database
        /// </summary>
        private void CommitCurrentEntityUpdates()
        {
            if (GlobalSession.IsOpen)
                GlobalSession.Close();

            using (ISession session = SessionFactory.OpenSession())
            {
                var transaction = session.BeginTransaction();
                foreach (Entity entity in EntitiesToPersist)
                {
                    try
                    {
                        session.SaveOrUpdate(entity);
                    }
                    catch (Exception e)
                    {
                        logger.Log(LogLevel.Error, "Save or Update of Entity failed", e);
                    }
                }
                try
                {
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Transaction to persist entities failed: " + e.Message);
                    transaction.Rollback();
                }
                finally
                {
                    session.Close();
                }
            }
        }

        /// <summary>
        /// Converts an object to a byte array that can the be persisted as field value for an attribute
        /// </summary>
        /// <param name="obj">The value to be converted</param>
        /// <returns>The byte array representation of the object</returns>
        private byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        /// <summary>
        /// Flushes the queue of stored attribute updates and commits the changes to the database
        /// </summary>
        private void CommitCurrentAttributeUpdates()
        {
            if (GlobalSession.IsOpen)
                GlobalSession.Close();

            using (IStatelessSession session = SessionFactory.OpenStatelessSession())
            {
                var transaction = session.BeginTransaction();
                foreach (KeyValuePair<Guid, Dictionary<string, object>> componentUpdate in ComponentAttributesToPersist)
                {
                    Guid componentGuid = componentUpdate.Key;
                    foreach(KeyValuePair<string, object> attributeUpdate in componentUpdate.Value)
                    {
                        String updateQuery = "update attributes_to_components set value = :newValue where componentID = :componentGuid AND attributeName = :attributeName";
                        IQuery sqlQuery = session.CreateSQLQuery(updateQuery)
                            .SetBinary("newValue", ObjectToByteArray(attributeUpdate.Value))
                            .SetParameter("componentGuid", componentGuid)
                            .SetParameter("attributeName", attributeUpdate.Key);
                        sqlQuery.ExecuteUpdate();
                    }
                }
                try
                {
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    logger.WarnException("Failed to update Attribute", e);
                    transaction.Rollback();
                }
                finally
                {
                    session.Close();
                }
            }
        }

        /// <summary>
        /// Adds an enitity update to the list of entities that are queued to be persisted
        /// </summary>
        /// <param name="changedEntity">The entity to be persisted</param>
        internal void AddEntityToPersisted(Entity changedEntity) {
            lock (entityQueueLock)
            {
                if (!EntitiesToPersist.Contains(changedEntity))
                    EntitiesToPersist.Add(changedEntity);
            }
        }

        /// <summary>
        /// Adds an attribute update to the list of attribute that are queued to be persisted
        /// </summary>
        /// <param name="changedComponentGuid">Guid of the Attribute that has changed</param>
        /// <param name="newValue">New value of the changed attribute</param>
        private void AddAttributeToPersisted(Guid changedComponentGuid, string attributeName, object newValue)
        {
            lock (attributeQueueLock)
            {
                if (!ComponentAttributesToPersist.ContainsKey(changedComponentGuid))
                    ComponentAttributesToPersist.Add(changedComponentGuid, new Dictionary<string, object>());
                else
                    ComponentAttributesToPersist[changedComponentGuid][attributeName] = newValue;
            }
        }

        /// <summary>
        /// Removes an entity from database.
        /// </summary>
        /// <param name="entity">Entity.</param>
        private void RemoveEntityFromDatabase(Entity entity) {
            using (ISession session = SessionFactory.OpenSession()) {
                var transaction = session.BeginTransaction ();
                session.Delete (entity);
                transaction.Commit ();
            }
        }

        /// <summary>
        /// Retrieves the entities from database.
        /// </summary>
        internal void RetrieveEntitiesFromDatabase()
        {
            IList<Entity> entitiesInDatabase = new List<Entity> ();
            using (ISession session = SessionFactory.OpenSession())
            {
                entitiesInDatabase = session.CreateQuery("from " + typeof(Entity)).List<Entity>();
                foreach (Entity e in entitiesInDatabase)
                {
                    //if (e.Parent == null)
                    {
                        EntitiesToInitialize.Add(e.Guid);
                        World.Instance.Add(e);
                    }
                }
            }
        }

        #endregion

        private Configuration NHibernateConfiguration = new Configuration();
        internal ISessionFactory SessionFactory;
        private HashedSet<Guid> EntitiesToInitialize = new HashedSet<Guid>();
        private object entityQueueLock = new object();
        private object attributeQueueLock = new object();
        private List<Entity> EntitiesToPersist = new List<Entity>();
        private Dictionary<Guid, Dictionary<string, object>> ComponentAttributesToPersist = new Dictionary<Guid, Dictionary<string, object>>();
        internal ISession GlobalSession;
        internal readonly Guid pluginGuid = new Guid("d51e4394-68cc-4801-82f2-6b2a865b28df");

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}
