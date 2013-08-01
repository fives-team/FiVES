using System;
using NHibernate.Cfg;
using FIVES;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;
using System.Diagnostics;

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

		[SetUp()]
		public void initPersistenceTest()
		{
            cfg = new Configuration ();
            cfg.Configure ();

            sessionFactory = cfg.BuildSessionFactory ();

			componentRegistry = ComponentRegistry.Instance;
			entityRegistry = EntityRegistry.Instance;

		}
		[Test()]
		public void testSchemeGeneration()
		{
			cfg.AddAssembly (typeof(Entity).Assembly);
            new SchemaUpdate (cfg).Execute (true, true);
		}


        [Test()]
        public void shouldAddAndPersistComponent()
        {
            ComponentLayout layout = new ComponentLayout();
            layout["IntAttribute"] = AttributeType.INT;
            layout["StringAttribute"] = AttributeType.STRING;
            componentRegistry.defineComponent("myComponent", Guid.NewGuid(), layout);
            

            Entity entity = new Entity();
            entityRegistry.addEntity(entity);
            entity["myComponent"].setIntAttribute("IntAttribute", 42);
            entity["myComponent"].setStringAttribute("StringAttribute", "Hello World!");

            var session = sessionFactory.OpenSession ();
            var trans = session.BeginTransaction ();
            session.Save (entity);
            trans.Commit ();
        }

        [Test()]
        public void shouldAddAndSaveParentEntity()
        {
            Entity entity = new Entity();
            Entity childEntity = new Entity ();
            Assert.True(entity.addChildNode (childEntity));

            var entityGuid = entityRegistry.addEntity (entity);
            var childGuid = entityRegistry.addEntity (childEntity);

            Console.WriteLine ("Entity Guid: " + entityGuid);
            Console.WriteLine ("Child  Guid: " + childGuid);

            var session = sessionFactory.OpenSession ();
            var trans = session.BeginTransaction ();
            session.Save (entity);
            session.Save (childEntity);
            trans.Commit ();
        }

        [Test()]
        public void shouldPersistComponentRegistry ()
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
        }
	}
}

