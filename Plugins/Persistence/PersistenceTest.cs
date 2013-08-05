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
            layout["IntAttribute"] = AttributeType.INT;
            layout["StringAttribute"] = AttributeType.STRING;

            componentRegistry.defineComponent("myComponent", Guid.NewGuid(), layout);

            Entity entity = new Entity();
            Guid entityGuid = entityRegistry.addEntity(entity);
            entity["myComponent"].setIntAttribute("IntAttribute", 42);
            entity["myComponent"].setStringAttribute("StringAttribute", "Hello World!");

            var session = sessionFactory.OpenSession ();
            var trans = session.BeginTransaction ();
            session.Save (entity);
            trans.Commit ();

            entityRegistry.removeEntity (entityGuid);

            PersistencePlugin plugin = new PersistencePlugin ();
            plugin.initialize();
            plugin.retrieveEntitiesFromDatabase ();

            Entity storedEntity = entityRegistry.getEntityByGuid (entityGuid);
            Assert.IsTrue (storedEntity ["myComponent"].getIntAttribute ("IntAttribute") == 42);
            Assert.IsTrue (storedEntity ["myComponent"].getStringAttribute ("StringAttribute") == "Hello World!");

        }

        [Test()]
        public void shouldStoreAndRetrieveEntities()
        {
            Entity entity = new Entity();
            Entity childEntity = new Entity ();
            Assert.True(entity.addChildNode (childEntity));

            var entityGuid = entityRegistry.addEntity (entity);
            var childGuid = entityRegistry.addEntity (childEntity);

            Console.WriteLine ("Entity Guid: " + entityGuid);
            Console.WriteLine ("Child  Guid: " + childGuid);

            // Transfer entities to Database
            var session = sessionFactory.OpenSession ();
            var trans = session.BeginTransaction ();
            session.Save (entity);
            session.Save (childEntity);
            trans.Commit ();

            entityRegistry.removeEntity (entityGuid);
            entityRegistry.removeEntity (childGuid);

            PersistencePlugin plugin = new PersistencePlugin ();
            plugin.initialize();
            plugin.retrieveEntitiesFromDatabase ();

            List<Guid> guidsInRegistry = entityRegistry.getAllGUIDs ();
            Console.WriteLine (guidsInRegistry.ToString ());

            Assert.Contains (entityGuid, guidsInRegistry);
            Assert.Contains (childGuid, guidsInRegistry);
            Assert.IsTrue (entityRegistry.getEntityByGuid (childGuid).parent.Guid == entityGuid);
        }

        [Test()]
        public void shouldStoreAndRetrieveComponentRegistry ()
        {
            if(!componentRegistry.isRegistered("myComponent"))
            {
                ComponentLayout layout = new ComponentLayout();
                layout["IntAttribute"] = AttributeType.INT;
                layout["StringAttribute"] = AttributeType.STRING;
                componentRegistry.defineComponent("myComponent", Guid.NewGuid(), layout);
            }

            ComponentRegistryPersistence persist = new ComponentRegistryPersistence ();
            persist.getComponentsFromRegistry ();

            var session = sessionFactory.OpenSession ();
            var trans = session.BeginTransaction ();
            session.Save (persist);
            trans.Commit ();

            persist.registerPersistedComponents ();

            Assert.IsTrue (componentRegistry.getAttributeType ("myComponent", "IntAttribute") == AttributeType.INT);
            Assert.IsTrue (componentRegistry.getAttributeType ("myComponent", "StringAttribute") == AttributeType.STRING);
        }
	}
}

