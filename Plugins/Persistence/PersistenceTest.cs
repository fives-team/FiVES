using System;
using NHibernate.Cfg;
using FIVES;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;

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
			cfg.AddAssembly (typeof(Component).Assembly);
            new SchemaExport (cfg).Execute (true, true, false);
		}


        [Test()]
        public void shouldAddAndPersistComponent()
        {
            ComponentLayout layout = new ComponentLayout();
            layout["attr"] = AttributeType.INT;
            componentRegistry.defineComponent("myComponent", Guid.NewGuid(), layout);
            Component newComponent = componentRegistry.createComponent ("myComponent");

            Entity entity = new Entity();
            entityRegistry.addEntity(entity);
            entity["myComponent"].setIntAttribute("attr", 42);

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

            var session = sessionFactory.OpenSession ();
            var trans = session.BeginTransaction ();
            session.Save (entity);
            session.Save (childEntity);
            trans.Commit ();
        }
	}
}

