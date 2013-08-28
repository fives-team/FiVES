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

        public string getName()
        {
            return "Persistance";
        }

        public List<string> getDependencies()
        {
            return new List<string>();
        }

        public void initialize()
        {
            initializeNHibernate ();
            initializePersistedCollections ();
            EntityRegistry.Instance.OnEntityAdded += onEntityAdded;
        }

        public void onEntityAdded(Object sender, EntityAddedOrRemovedEventArgs e) {
            Entity addedEntity = EntityRegistry.Instance.getEntity (e.elementId);
            addedEntity.OnAttributeInComponentChanged += onComponentChanged;
            // Only persist entities if they are not added during intialization on Startup
            if (!entitiesToInitialize.Contains (e.elementId)) {
                persistEntityToDatabase (addedEntity);
            } else {
                entitiesToInitialize.Remove (e.elementId);
            }
        }

        public void onComponentChanged(Object sender, AttributeInComponentEventArgs e) {
            Entity changedEntity = (Entity)sender;
            Component changedComponent = changedEntity [e.componentName];
            // TODO: change cascading persistence of entity, but only persist component and take care to persist mapping to entity as well
            persistEntityToDatabase (changedEntity);
        }

        #endregion

        private void initializeNHibernate() {
            nHibernateConfiguration.Configure ();
            nHibernateConfiguration.AddAssembly (typeof(Entity).Assembly);
            sessionFactory = nHibernateConfiguration.BuildSessionFactory ();
            globalSession = sessionFactory.OpenSession ();
        }

        private void initializePersistedCollections() {
            retrieveComponentRegistryFromDatabase ();
            retrieveEntitiesFromDatabase ();
        }

        private void persistEntityToDatabase(Entity addedEntity) {

            using(ISession session = sessionFactory.OpenSession()) {
                var transaction = session.BeginTransaction ();
                session.SaveOrUpdate (addedEntity);
                transaction.Commit ();
            }
        }

        private void persistComponentToDatabase(Component component) {
            using(ISession session = sessionFactory.OpenSession()) {
                var transaction = session.BeginTransaction ();
                session.SaveOrUpdate (component);
                transaction.Commit ();
            }
        }

        internal void retrieveComponentRegistryFromDatabase()
        {
            ComponentRegistryPersistence persistedRegistry = null;
            using(ISession session = sessionFactory.OpenSession())
                session.Get<ComponentRegistryPersistence> (ComponentRegistry.Instance.RegistryGuid);
            if(persistedRegistry != null)
                persistedRegistry.registerPersistedComponents ();

        }

        internal void retrieveEntitiesFromDatabase()
        {
            IList<Entity> entitiesInDatabase = new List<Entity> ();
            entitiesInDatabase = globalSession.CreateQuery ("from " + typeof(Entity)).List<Entity> ();
            foreach (Entity e in entitiesInDatabase) {
                entitiesToInitialize.Add (e.Guid);
                EntityRegistry.Instance.addEntity (e);
            }
        }

        private Configuration nHibernateConfiguration = new Configuration();
        private ISessionFactory sessionFactory;
        private ISession globalSession;
        private HashedSet<Guid> entitiesToInitialize = new HashedSet<Guid>();
	}
}
