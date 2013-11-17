using System;
using NHibernate.Cfg;
using FIVES;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;

namespace PersistencePlugin
{
    [Ignore()]
    [TestFixture()]
    public class PersistenceTest
    {
        ComponentRegistry componentRegistry;
        EntityCollection entityRegistry;
        Configuration cfg;
        NHibernate.ISessionFactory sessionFactory;
        PersistencePlugin plugin;

        public PersistenceTest ()
        {

        }

        [SetUp()]
        public void SetUpDatabaseTest() {
            entityRegistry = World.Instance;
            componentRegistry = ComponentRegistry.Instance;
        }

        [Test()]
        public void FirstShouldSetupDatabase()
        {
            cfg = new Configuration ();
            cfg.Configure ();

            sessionFactory = cfg.BuildSessionFactory ();

            cfg.AddAssembly (typeof(Entity).Assembly);
            new SchemaExport (cfg).Execute (true, true, false);
        }

        [Test()]
        public void ShouldStoreAndRetrieveComponent()
        {
            ComponentDefinition myComponent = new ComponentDefinition("myComponent");
            myComponent.AddAttribute<int>("IntAttribute");
            myComponent.AddAttribute<string>("StringAttribute");
            componentRegistry.Register(myComponent);

            if (plugin == null) {
                plugin = new PersistencePlugin ();
                plugin.Initialize ();
            }

            Entity entity = new Entity();

            entityRegistry.Add(entity);
            entity["myComponent"]["IntAttribute"] = 42;
            entity["myComponent"]["StringAttribute"] = "Hello World!";

            // De-Activate on-remove event handler, as for tests, we only want to remove the entity from the local registry, not from the
            // persistence storage
            entityRegistry.RemovedEntity -= plugin.OnEntityRemoved;
            entityRegistry.Remove (entity);

            plugin.RetrieveEntitiesFromDatabase ();

            Entity storedEntity = entityRegistry.FindEntity(entity.Guid.ToString());
            Assert.AreEqual(42, storedEntity["myComponent"]["IntAttribute"]);
            Assert.AreEqual("Hello World!", storedEntity["myComponent"]["StringAttribute"]);
            Assert.AreEqual(1, storedEntity["myComponent"].Definition.Version);
        }

        [Test()]
        public void ShouldStoreAndRetrieveEntities()
        {
            Entity entity = new Entity();

            if (plugin == null) {
                plugin = new PersistencePlugin ();
                plugin.Initialize ();
            }


            entityRegistry.Add(entity);

            plugin.RetrieveEntitiesFromDatabase ();

            Assert.True(entityRegistry.Contains(entity));
        }

        [Test()]
        public void ShouldStoreAndRetrieveComponentRegistry ()
        {
            if(componentRegistry.FindComponentDefinition("myComponent") != null)
            {
                ComponentDefinition myComponent = new ComponentDefinition("myComponent");
                myComponent.AddAttribute<int>("IntAttribute");
                myComponent.AddAttribute<string>("StringAttribute");
                componentRegistry.Register(myComponent);
            }

            ComponentRegistryPersistence persist = new ComponentRegistryPersistence ();
            persist.GetComponentsFromRegistry ();

            var session = sessionFactory.OpenSession ();
            var trans = session.BeginTransaction ();
            session.Save (persist);
            trans.Commit ();

            plugin.RetrieveComponentRegistryFromDatabase ();

            ReadOnlyComponentDefinition myComponentDef = componentRegistry.FindComponentDefinition("myComponent");
            Assert.AreEqual(typeof(int), myComponentDef["IntAttribute"].Type);
            Assert.AreEqual(typeof(string), myComponentDef["StringAttribute"].Type);
        }

        [Test()]
        public void ShouldDeleteEntity()
        {
            Entity entity = new Entity();

            if (plugin == null) {
                plugin = new PersistencePlugin ();
                plugin.Initialize ();
            }

            entityRegistry.Add (entity);
            entityRegistry.Remove (entity);

            if (!plugin.GlobalSession.IsOpen)
                plugin.GlobalSession = plugin.SessionFactory.OpenSession();
            plugin.RetrieveEntitiesFromDatabase ();

            Assert.True(entityRegistry.Contains(entity));
        }

/*        public void ShouldPersistUpgradedComponentLayout() {

            ComponentLayout layout_1 = new ComponentLayout ();
            ComponentLayout layout_2 = new ComponentLayout ();

            layout_1 ["i"] = typeof(int);
            layout_1 ["f"] = typeof(float);

            layout_2 ["i"] = typeof(float);
            layout_2 ["f"] = typeof(int);
            layout_2 ["b"] = typeof(bool);

            componentRegistry.defineComponent ("Comp1", plugin.pluginGuid, layout_1);

            ComponentRegistryPersistence persist = new ComponentRegistryPersistence ();
            persist.getComponentsFromRegistry ();

            var session = sessionFactory.OpenSession ();
            var trans = session.BeginTransaction ();
            session.Save (persist);
            trans.Commit ();

            var newEntity = new Entity ();

        }
*/
        [Test()]
        public void ShouldPersistUpgradedEntity() {

            string name = "ComponentToUpgrade";

            ComponentDefinition componentToUpgrade = new ComponentDefinition(name);
            componentToUpgrade.AddAttribute<int>("i");
            componentToUpgrade.AddAttribute<float>("f");
            componentToUpgrade.AddAttribute<string>("s");
            componentToUpgrade.AddAttribute<bool>("b");
            componentRegistry.Register(componentToUpgrade);

            Entity entity = new Entity ();

            entityRegistry.Add (entity);
            entity[name]["i"] = 42;
            entity[name]["f"] = 3.14f;
            entity[name]["s"] = "foobar";
            entity[name]["b"] = false;

            ComponentDefinition componentToUpgrade2 = new ComponentDefinition(name, 2);
            componentToUpgrade2.AddAttribute<int>("i");
            componentToUpgrade2.AddAttribute<float>("f");
            componentToUpgrade2.AddAttribute<string>("s");
            componentToUpgrade2.AddAttribute<bool>("b");
            componentRegistry.Upgrade(componentToUpgrade2, TestUpgrader);

            plugin.RetrieveEntitiesFromDatabase ();

            Assert.IsTrue(entityRegistry.Contains(entity));

            Entity retrievedEntity = entityRegistry.FindEntity (entity.Guid.ToString());

            Assert.AreEqual(retrievedEntity[name]["i"], 3);
            Assert.AreEqual(retrievedEntity[name]["f"], 42);
            Assert.IsNull(retrievedEntity[name]["s"]);
            Assert.AreEqual(retrievedEntity[name]["b"], false);

        }

        public static void TestUpgrader(Component oldComponent, Component newComponent) {
            newComponent["f"] = (float)(int)oldComponent["i"];
            newComponent["i"] = (int)(float)oldComponent["f"];
            newComponent["b"] = oldComponent["b"];
        }
    }
}

