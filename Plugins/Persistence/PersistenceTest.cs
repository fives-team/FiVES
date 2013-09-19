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
        public void SetUpDatabaseTest() {
            entityRegistry = EntityRegistry.Instance;
            componentRegistry = ComponentRegistry.Instance;
        }

        [Test()]
        public void ShouldSetupDatabase()
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
            ComponentLayout layout = new ComponentLayout();
            layout.AddAttribute<int>("IntAttribute");
            layout.AddAttribute<string>("StringAttribute");

            componentRegistry.DefineComponent("myComponent", Guid.NewGuid(), layout);

            if (plugin == null) {
                Console.WriteLine (" ==== [SHOULD STORE AND RETRIEVE COMPONENTS]: Initializing Plugin");
                plugin = new PersistencePlugin ();
                plugin.Initialize ();
            }

            Entity entity = new Entity();

            Console.WriteLine (" ==== [SHOULD STORE AND RETRIEVE COMPONENTS]: Adding Entity " + entity.Guid);
            entityRegistry.AddEntity(entity);
            Console.WriteLine (" ==== [SHOULD STORE AND RETRIEVE COMPONENTS]: Setting Attributes for " + entity.Guid);
            entity["myComponent"]["IntAttribute"] = 42;
            entity["myComponent"]["StringAttribute"] = "Hello World!";

            // De-Activate on-remove event handler, as for tests, we only want to remove the entity from the local registry, not from the
            // persistence storage
            entityRegistry.OnEntityRemoved -= plugin.OnEntityRemoved;
            entityRegistry.RemoveEntity (entity.Guid);

            plugin.RetrieveEntitiesFromDatabase ();
            Entity storedEntity = entityRegistry.GetEntity(entity.Guid);            Assert.IsTrue ((int) entity["myComponent"]["IntAttribute"] == 42);            Assert.IsTrue ((string) entity["myComponent"]["StringAttribute"] == "Hello World!");
            Assert.AreEqual(1, storedEntity["myComponent"].Version);
        }

        [Test()]
        public void ShouldStoreAndRetrieveEntities()
        {
            Entity entity = new Entity();
            Entity childEntity = new Entity ();
            Assert.True(entity.AddChildNode (childEntity));

            if (plugin == null) {
                Console.WriteLine (" ==== [SHOULD STORE AND RETRIEVE ENTITIES]: Initializing Plugin ");
                plugin = new PersistencePlugin ();
                plugin.Initialize ();
            }

            Console.WriteLine (" ==== [SHOULD STORE AND RETRIEVE ENTITIES]: Adding Entity " + entity.Guid);
            entityRegistry.AddEntity (entity);
            Console.WriteLine (" ==== [SHOULD STORE AND RETRIEVE ENTITIES]: Adding Entity " + childEntity.Guid);
            entityRegistry.AddEntity (childEntity);

            // De-Activate on-remove event handler, as for tests, we only want to remove the entity from the local registry, not from the
            // persistence storage
            entityRegistry.OnEntityRemoved -= plugin.OnEntityRemoved;
            entityRegistry.RemoveEntity (childEntity.Guid);
            entityRegistry.RemoveEntity (entity.Guid);

            plugin.RetrieveEntitiesFromDatabase ();

            ISet<Guid> guidsInRegistry = entityRegistry.GetAllGUIDs ();
            Console.WriteLine (guidsInRegistry.ToString ());

            Assert.True(guidsInRegistry.Contains(entity.Guid));
            Assert.True(guidsInRegistry.Contains(childEntity.Guid));
            Assert.IsTrue (entityRegistry.GetEntity (childEntity.Guid).Parent.Guid == entity.Guid);
        }

        [Test()]
        public void ShouldStoreAndRetrieveComponentRegistry ()
        {
            if(!componentRegistry.IsRegistered("myComponent"))
            {
                ComponentLayout layout = new ComponentLayout();
                layout.AddAttribute<int>("IntAttribute");
                layout.AddAttribute<string>("StringAttribute");
                componentRegistry.DefineComponent("myComponent", Guid.NewGuid(), layout);
            }

            ComponentRegistryPersistence persist = new ComponentRegistryPersistence ();
            persist.GetComponentsFromRegistry ();

            var session = sessionFactory.OpenSession ();
            var trans = session.BeginTransaction ();
            session.Save (persist);
            trans.Commit ();

            plugin.RetrieveComponentRegistryFromDatabase ();

            Assert.IsTrue (componentRegistry.GetAttributeType ("myComponent", "IntAttribute") == typeof(int));
            Assert.IsTrue (componentRegistry.GetAttributeType ("myComponent", "StringAttribute") == typeof(string));
        }

        [Test()]
        public void ShouldDeleteEntity()
        {
            Entity entity = new Entity();
            Entity childEntity = new Entity ();
            Assert.True(entity.AddChildNode (childEntity));

            if (plugin == null) {
                Console.WriteLine (" ==== [SHOULD STORE AND RETRIEVE ENTITIES]: Initializing Plugin ");
                plugin = new PersistencePlugin ();
                plugin.Initialize ();
            }

            Console.WriteLine (" ==== [SHOULD STORE AND RETRIEVE ENTITIES]: Adding Entity " + entity.Guid);
            entityRegistry.AddEntity (entity);
            Console.WriteLine (" ==== [SHOULD STORE AND RETRIEVE ENTITIES]: Adding Entity " + childEntity.Guid);
            entityRegistry.AddEntity (childEntity);

            entityRegistry.RemoveEntity (childEntity.Guid);
            entityRegistry.RemoveEntity (entity.Guid);

            plugin.RetrieveEntitiesFromDatabase ();

            ISet<Guid> guidsInRegistry = entityRegistry.GetAllGUIDs ();
            Console.WriteLine (guidsInRegistry.ToString ());

            Assert.True(!guidsInRegistry.Contains(entity.Guid));
            Assert.True(!guidsInRegistry.Contains(childEntity.Guid));
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

            ComponentLayout layout = new ComponentLayout ();
            layout.AddAttribute<int> ("i");
            layout.AddAttribute<float> ("f");
            layout.AddAttribute<string> ("s");
            layout.AddAttribute<bool> ("b");

            string name = "ComponentToUpgrade";
            componentRegistry.DefineComponent (name, plugin.pluginGuid, layout);
            Entity entity = new Entity ();
            Guid entityGuid = entity.Guid;

            entityRegistry.AddEntity (entity);
            entity[name]["i"] = 42;
            entity[name]["f"] = 3.14f;
            entity[name]["s"] = "foobar";
            entity[name]["b"] = false;

            componentRegistry.UpgradeComponent(name, plugin.pluginGuid, layout, 2, TestUpgrader);
            entityRegistry.OnEntityRemoved -= plugin.OnEntityRemoved;
            entityRegistry.RemoveEntity (entityGuid);

            plugin.RetrieveEntitiesFromDatabase ();

            ISet<Guid> guidsInRegistry = entityRegistry.GetAllGUIDs ();
            Assert.IsTrue(guidsInRegistry.Contains(entityGuid));

            Entity retrievedEntity = entityRegistry.GetEntity (entityGuid);

            Assert.AreEqual(retrievedEntity[name]["i"], 3);
            Assert.AreEqual(retrievedEntity[name]["f"], 42);
            Assert.IsNull(retrievedEntity[name]["s"]);
            Assert.AreEqual(retrievedEntity[name]["b"], false);

        }

        public static void TestUpgrader(Component oldComponent, ref Component newComponent) {
            newComponent["f"] = (float)(int)oldComponent["i"];
            newComponent["i"] = (int)(float)oldComponent["f"];
            newComponent["b"] = oldComponent["b"];
        }
    }
}

