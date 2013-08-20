using System;
using FIVES;
using NHibernate.Cfg;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using System.Collections.Generic;


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
            sessionFactory = nHibernateConfiguration.BuildSessionFactory ();
            nHibernateConfiguration.AddAssembly (typeof(Entity).Assembly);
        }

        #endregion

        internal void retrieveComponentRegistryFromDatabase()
        {
            var session = sessionFactory.OpenSession ();
            ComponentRegistryPersistence persistedRegistry = session.Get<ComponentRegistryPersistence> (ComponentRegistry.Instance.RegistryGuid);
            persistedRegistry.registerPersistedComponents ();

        }

        internal void retrieveEntitiesFromDatabase()
        {
            var session = sessionFactory.OpenSession ();
            IList<Entity> entitiesInDatabase = session.CreateQuery ("from " + typeof(Entity)).List<Entity> ();
            foreach (Entity e in entitiesInDatabase)
                EntityRegistry.Instance.addEntity(e);
        }

        private Configuration nHibernateConfiguration = new Configuration();
        private ISessionFactory sessionFactory;
	}
}
