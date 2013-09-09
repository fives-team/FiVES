using System;
using FIVES;
using NHibernate.Cfg;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using System.Collections.Generic;
using Events;
using System.Diagnostics;
using Iesi.Collections.Generic;


namespace Persistence
{
    public class PersistencePlugin: IPluginInitializer
    {
        #region IPluginInitializer implementation

        /// <summary>
        /// Returns the name of the plugin.
        /// </summary>
        /// <returns>The name of the plugin.</returns>
        public string getName()
        {
            return "Persistence";
        }

        /// <summary>
        /// Returns the list of names of the plugins that this plugin depends on.
        /// </summary>
        /// <returns>The list of names of the plugins that this plugin depends on.</returns>
        public List<string> getDependencies()
        {
            return new List<string>();
        }

        /// <summary>
        /// Initializes the plugin. This method will be called by the plugin manager when all dependency plugins have
        /// been loaded.
        /// </summary>
        public void initialize()
        {
            initializeNHibernate ();
            initializePersistedCollections ();
            EntityRegistry.Instance.OnEntityAdded += onEntityAdded;
            EntityRegistry.Instance.OnEntityRemoved += onEntityRemoved;
            ComponentRegistry.Instance.OnEntityComponentUpgraded += onComponentOfEntityUpgraded;
        }

        /// <summary>
        /// Initializes NHibernate. Adds the mappings based on the assembly, initializes the session factory and opens
        /// a long-running global session
        /// </summary>
        private void initializeNHibernate() {
            nHibernateConfiguration.Configure ();
            nHibernateConfiguration.AddAssembly (typeof(Entity).Assembly);
            new SchemaUpdate (nHibernateConfiguration).Execute(false, true);
            sessionFactory = nHibernateConfiguration.BuildSessionFactory ();
            globalSession = sessionFactory.OpenSession ();
        }

        /// <summary>
        /// Initializes the state of the system as stored in the database by retrieving the stored component registry and
        /// the stored entities
        /// </summary>
        private void initializePersistedCollections() {
            retrieveComponentRegistryFromDatabase ();
            retrieveEntitiesFromDatabase ();
        }

        #endregion

        #region Event Handlers
        /// <summary>
        /// Event Handler for EntityAdded event fired by the EntityRegistry. A new entity will be persisted to the database,
        /// unless the entity to be added was just queried from the database (e.g. upon initialization)
        /// </summary>
        /// <param name="sender">Sender of the event (the EntityRegistry)</param>
        /// <param name="e">Event arguments</param>
        internal void onEntityAdded(Object sender, EntityAddedOrRemovedEventArgs e) {
            Entity addedEntity = EntityRegistry.Instance.getEntity (e.elementId);
            addedEntity.OnAttributeInComponentChanged += onComponentChanged;
            // Only persist entities if they are not added during intialization on Startup
            if (!entitiesToInitialize.Contains (e.elementId)) {
                persistEntityToDatabase (addedEntity);
            } else {
                entitiesToInitialize.Remove (e.elementId);
            }
        }

        /// <summary>
        /// Event Handler for EntityRemoved event fired by the entity registry. Once an entity is removed from the registry,
        /// its entry is also deleted from the database (including cascaded deletion of its component instances)
        /// </summary>
        /// <param name="sender">Sender of the event (the EntityRegistry)</param>
        /// <param name="e">Event Arguments</param>
        internal void onEntityRemoved(Object sender, EntityAddedOrRemovedEventArgs e) {
            Entity entityToRemove = EntityRegistry.Instance.getEntity (e.elementId);
            removeEntityFromDatabase (entityToRemove);
        }

        /// <summary>
        /// Event Handler for AttributeInComponentChanged on Entity. When fired by the entity, the entity is persisted to the
        /// data base, with casacading save of components. The whole entity is persisted (instead of only the component), to
        /// correctly store the link of the component to the entity on newly instantiated components as well.
        /// </summary>
        /// <param name="sender">Sender of the event (the Entity)</param>
        /// <param name="e">Event arguments</param>
        internal void onComponentChanged(Object sender, AttributeInComponentEventArgs e) {
            Entity changedEntity = (Entity)sender;
            Component changedComponent = changedEntity [e.componentName];
            // TODO: change cascading persistence of entity, but only persist component and take care to persist mapping to entity as well
            persistEntityToDatabase (changedEntity);
        }

        internal void onComponentOfEntityUpgraded(Object sender, EntityComponentUpgradedEventArgs e) {

            // TODO: change cascading persistence of entity, but only persist component and take care to persist mapping to entity as well
            persistEntityToDatabase (e.entity);
        }
        #endregion

        #region database synchronisation

        /// <summary>
        /// Persists an entity to database.
        /// </summary>
        /// <param name="addedEntity">Added entity</param>
        private void persistEntityToDatabase(Entity addedEntity) {

            using(ISession session = sessionFactory.OpenSession()) {
                var transaction = session.BeginTransaction ();
                session.SaveOrUpdate (addedEntity);
                transaction.Commit ();
            }
        }

        /// <summary>
        /// Removes an entity from database.
        /// </summary>
        /// <param name="entity">Entity.</param>
        private void removeEntityFromDatabase(Entity entity) {
            using (ISession session = sessionFactory.OpenSession()) {
                var transaction = session.BeginTransaction ();
                session.Delete (entity);
                transaction.Commit ();
            }
        }

        /// <summary>
        /// Persists the component to database.
        /// </summary>
        /// <param name="component">Component.</param>
        private void persistComponentToDatabase(Component component) {
            using(ISession session = sessionFactory.OpenSession()) {
                var transaction = session.BeginTransaction ();
                session.SaveOrUpdate (component);
                transaction.Commit ();
            }
        }

        /// <summary>
        /// Retrieves the component registry from database.
        /// </summary>
        internal void retrieveComponentRegistryFromDatabase()
        {
            ComponentRegistryPersistence persistedRegistry = null;
            using(ISession session = sessionFactory.OpenSession())
                session.Get<ComponentRegistryPersistence> (ComponentRegistry.Instance.RegistryGuid);
            if(persistedRegistry != null)
                persistedRegistry.registerPersistedComponents ();

        }

        /// <summary>
        /// Retrieves the entities from database.
        /// </summary>
        internal void retrieveEntitiesFromDatabase()
        {
            IList<Entity> entitiesInDatabase = new List<Entity> ();
            entitiesInDatabase = globalSession.CreateQuery ("from " + typeof(Entity)).List<Entity> ();
            foreach (Entity e in entitiesInDatabase) {
                entitiesToInitialize.Add (e.Guid);
                EntityRegistry.Instance.addEntity (e);
            }
        }

        #endregion
        private Configuration nHibernateConfiguration = new Configuration();
        private ISessionFactory sessionFactory;
        private ISession globalSession;
        private HashedSet<Guid> entitiesToInitialize = new HashedSet<Guid>();

        internal readonly Guid pluginGuid = new Guid("d51e4394-68cc-4801-82f2-6b2a865b28df");
    }
}
