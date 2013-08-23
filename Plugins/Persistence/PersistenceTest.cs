using System;
using NHibernate.Cfg;
using FIVES;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;
using System.Diagnostics;
using System.Collections.Generic;

namespace Persistence
{
	[TestFixture()]
	public class PersistenceTest
	{
		ComponentRegistry componentRegistry;
		EntityRegistry entityRegistry;
        Configuration cfg;
        NHibernate.ISessionFactory sessionFactory;

		public PersistenceTest ()
		{
		}

        [Test()]
        public void shouldSetupDatabase()
        {
            cfg = new Configuration ();
            cfg.Configure ();

            sessionFactory = cfg.BuildSessionFactory ();

            componentRegistry = ComponentRegistry.Instance;
            entityRegistry = EntityRegistry.Instance;

            cfg.AddAssembly (typeof(Entity).Assembly);
            new SchemaExport (cfg).Execute (true, true, false);
        }

        [Test()]
        public void shouldStoreAndRetrieveComponent()
        {
            ComponentLayout layout = new ComponentLayout();
            layout["IntAttribute"] = typeof(int);
            layout["StringAttribute"] = typeof(string);

            componentRegistry.defineComponent("myComponent", Guid.NewGuid(), layout);

            dynamic entity = new Entity();
            entityRegistry.addEntity(entity);
            entity.myComponent.IntAttribute = 42;
            entity.myComponent.StringAttribute= "Hello World!";

            var session = sessionFactory.OpenSession ();
            var trans = session.BeginTransaction ();
            session.Save (entity);
            trans.Commit ();

            entityRegistry.removeEntity (entity.Guid);

            PersistencePlugin plugin = new PersistencePlugin ();
            plugin.initialize();
            plugin.retrieveEntitiesFromDatabase ();

            dynamic storedEntity = entityRegistry.getEntity(entity.Guid);
            Assert.IsTrue (storedEntity.myComponent.IntAttribute == 42);
            Assert.IsTrue (storedEntity.myComponent.StringAttribute == "Hello World!");

        }

        [Test()]
        public void shouldStoreAndRetrieveEntities()
        {
            Entity entity = new Entity();
            Entity childEntity = new Entity ();
            Assert.True(entity.addChildNode (childEntity));

            entityRegistry.addEntity (entity);
            entityRegistry.addEntity (childEntity);

            Console.WriteLine ("Entity Guid: " + entity.Guid);
            Console.WriteLine ("Child  Guid: " + childEntity.Guid);

            // Transfer entities to Database
            var session = sessionFactory.OpenSession ();
            var trans = session.BeginTransaction ();
            session.Save (entity);
            session.Save (childEntity);
            trans.Commit ();

            entityRegistry.removeEntity (entity.Guid);
            entityRegistry.removeEntity (childEntity.Guid);

            PersistencePlugin plugin = new PersistencePlugin ();
            plugin.initialize();
            plugin.retrieveEntitiesFromDatabase ();

            HashSet<Guid> guidsInRegistry = entityRegistry.getAllGUIDs ();
            Console.WriteLine (guidsInRegistry.ToString ());

            Assert.True(guidsInRegistry.Contains(entity.Guid));
            Assert.True(guidsInRegistry.Contains(childEntity.Guid));
            Assert.IsTrue (entityRegistry.getEntity (childEntity.Guid).parent.Guid == entity.Guid);
        }

        [Test()]
        public void shouldStoreAndRetrieveComponentRegistry ()
        {
            if(!componentRegistry.isRegistered("myComponent"))
            {
                ComponentLayout layout = new ComponentLayout();
                layout["IntAttribute"] = typeof(int);
                layout["StringAttribute"] = typeof(string);
                componentRegistry.defineComponent("myComponent", Guid.NewGuid(), layout);
            }

            ComponentRegistryPersistence persist = new ComponentRegistryPersistence ();
            persist.getComponentsFromRegistry ();

            var session = sessionFactory.OpenSession ();
            var trans = session.BeginTransaction ();
            session.Save (persist);
            trans.Commit ();

            persist.registerPersistedComponents ();

            Assert.IsTrue (componentRegistry.getAttributeType ("myComponent", "IntAttribute") == typeof(int));
            Assert.IsTrue (componentRegistry.getAttributeType ("myComponent", "StringAttribute") == typeof(string));
        }
	}
}

