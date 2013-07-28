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

		public PersistenceTest ()
		{
		}

		[SetUp()]
		public void initPersistenceTest()
		{
			componentRegistry = ComponentRegistry.Instance;
			entityRegistry = EntityRegistry.Instance;

		}
		[Test()]
		public void testSchemeGeneration()
		{
			Configuration cfg = new Configuration ();
			cfg.Configure ();
			cfg.AddAssembly (typeof(Entity).Assembly);
			cfg.AddAssembly (typeof(Component).Assembly);
			new SchemaExport (cfg).Execute (true, true, false);

			ComponentLayout layout = new ComponentLayout();
			layout["attr"] = AttributeType.INT;
			componentRegistry.defineComponent("myComponent", Guid.NewGuid(), layout);
			Component newComponent = componentRegistry.createComponent ("myComponent");

			var entity = new Entity();
			entityRegistry.addEntity(entity);
			entity["myComponent"].setIntAttribute("attr", 42);

			NHibernate.ISessionFactory sessionFactory = cfg.BuildSessionFactory ();
			var session = sessionFactory.OpenSession ();

			var trans2 = session.BeginTransaction ();
			session.Save (entity);
			trans2.Commit ();
		}
	}
}

