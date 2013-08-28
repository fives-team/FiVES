using System;
using FIVES;
using NHibernate.Cfg;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using System.Collections.Generic;
using Events;
using System.Diagnostics;


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
            nHibernateConfiguration.Configure ();
            ISessionFactory sessionFactory = nHibernateConfiguration.BuildSessionFactory ();
            session = sessionFactory.OpenSession ();
            retrieveComponentRegistryFromDatabase ();
            retrieveEntitiesFromDatabase ();
            nHibernateConfiguration.AddAssembly (typeof(Entity).Assembly);
            EntityRegistry.Instance.OnEntityAdded += new EntityRegistry.EntityAdded (onEntityAdded);
        }

        public void onEntityAdded(Object sender, EntityAddedOrRemovedEventArgs e) {
            Entity addedEntity = EntityRegistry.Instance.getEntity (e.elementId);
            var transaction = session.BeginTransaction ();
            session.Save (addedEntity);
            transaction.Commit ();
        }
        #endregion

        internal void retrieveComponentRegistryFromDatabase()
        {
            ComponentRegistryPersistence persistedRegistry = session.Get<ComponentRegistryPersistence> (ComponentRegistry.Instance.RegistryGuid);
            if(persistedRegistry != null)
                persistedRegistry.registerPersistedComponents ();

        }

        internal void retrieveEntitiesFromDatabase()
        {
            IList<Entity> entitiesInDatabase = session.CreateQuery ("from " + typeof(Entity)).List<Entity> ();
            foreach (Entity e in entitiesInDatabase) {
                EntityRegistry.Instance.addEntity (e);
            }
        }

        private Configuration nHibernateConfiguration = new Configuration();
        private ISessionFactory sessionFactory;
        private ISession globalSession;
	}
}
