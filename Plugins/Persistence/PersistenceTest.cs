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
        PersistencePlugin plugin;

		public PersistenceTest ()
		{

		}

        [SetUp()]
        public void setUpDatabaseTest() {
            entityRegistry = EntityRegistry.Instance;
            componentRegistry = ComponentRegistry.Instance;
        }

        [Test()]
        public void shouldSetupDatabase()
        {
            cfg = new Configuration ();
            cfg.Configure ();

            sessionFactory = cfg.BuildSessionFactory ();

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

            if (plugin == null) {
                Console.WriteLine (" ==== [SHOULD STORE AND RETRIEVE COMPONENTS]: Initializing Plugin");
                plugin = new PersistencePlugin ();
                plugin.initialize ();
            }

            dynamic entity = new Entity();

            Console.WriteLine (" ==== [SHOULD STORE AND RETRIEVE COMPONENTS]: Adding Entity " + entity.Guid);
            entityRegistry.addEntity(entity);
            Console.WriteLine (" ==== [SHOULD STORE AND RETRIEVE COMPONENTS]: Setting Attributes for " + entity.Guid);
            entity.myComponent.IntAttribute = 42;
            entity.myComponent.StringAttribute= "Hello World!";

            // De-Activate on-remove event handler, as for tests, we only want to remove the entity from the local registry, not from the
            // persistence storage
            entityRegistry.OnEntityRemoved -= plugin.onEntityRemoved;
            entityRegistry.removeEntity (entity.Guid);

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

            if (plugin == null) {
                Console.WriteLine (" ==== [SHOULD STORE AND RETRIEVE ENTITIES]: Initializing Plugin ");
                plugin = new PersistencePlugin ();
                plugin.initialize ();
            }

            Console.WriteLine (" ==== [SHOULD STORE AND RETRIEVE ENTITIES]: Adding Entity " + entity.Guid);
            entityRegistry.addEntity (entity);
            Console.WriteLine (" ==== [SHOULD STORE AND RETRIEVE ENTITIES]: Adding Entity " + childEntity.Guid);
            entityRegistry.addEntity (childEntity);

            // De-Activate on-remove event handler, as for tests, we only want to remove the entity from the local registry, not from the
            // persistence storage
            entityRegistry.OnEntityRemoved -= plugin.onEntityRemoved;
            entityRegistry.removeEntity (childEntity.Guid);
            entityRegistry.removeEntity (entity.Guid);

            plugin.retrieveEntitiesFromDatabase ();

            ISet<Guid> guidsInRegistry = entityRegistry.getAllGUIDs ();
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

        [Test()]
        public void shouldDeleteEntity()
        {
            Entity entity = new Entity();
            Entity childEntity = new Entity ();
            Assert.True(entity.addChildNode (childEntity));

            if (plugin == null) {
                Console.WriteLine (" ==== [SHOULD STORE AND RETRIEVE ENTITIES]: Initializing Plugin ");
                plugin = new PersistencePlugin ();
                plugin.initialize ();
            }

            Console.WriteLine (" ==== [SHOULD STORE AND RETRIEVE ENTITIES]: Adding Entity " + entity.Guid);
            entityRegistry.addEntity (entity);
            Console.WriteLine (" ==== [SHOULD STORE AND RETRIEVE ENTITIES]: Adding Entity " + childEntity.Guid);
            entityRegistry.addEntity (childEntity);

            entityRegistry.removeEntity (childEntity.Guid);
            entityRegistry.removeEntity (entity.Guid);

            plugin.retrieveEntitiesFromDatabase ();

            ISet<Guid> guidsInRegistry = entityRegistry.getAllGUIDs ();
            Console.WriteLine (guidsInRegistry.ToString ());

            Assert.True(!guidsInRegistry.Contains(entity.Guid));
            Assert.True(!guidsInRegistry.Contains(childEntity.Guid));
        }
	}
}

