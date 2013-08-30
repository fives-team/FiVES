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
            layout.addAttribute<int>("IntAttribute");
            layout.addAttribute<string>("StringAttribute");

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
            Assert.AreEqual(1, storedEntity.myComponent.Version);
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
                layout.addAttribute<int>("IntAttribute");
                layout.addAttribute<string>("StringAttribute");
                componentRegistry.defineComponent("myComponent", Guid.NewGuid(), layout);
            }

            ComponentRegistryPersistence persist = new ComponentRegistryPersistence ();
            persist.getComponentsFromRegistry ();

            var session = sessionFactory.OpenSession ();
            var trans = session.BeginTransaction ();
            session.Save (persist);
            trans.Commit ();

            plugin.retrieveComponentRegistryFromDatabase ();

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

/*        public void shouldPersistUpgradedComponentLayout() {

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

            dynamic newEntity = new Entity ();

        }
*/
        [Test()]
        public void shouldPersistUpgradedEntity() {

            ComponentLayout layout = new ComponentLayout ();
            layout["i"] = typeof(int);
            layout["f"] = typeof(float);
            layout["s"] = typeof(string);
            layout["b"] = typeof(bool);

            string name = "ComponentToUpgrade";
            componentRegistry.defineComponent (name, plugin.pluginGuid, layout);
            Entity entity = new Entity ();
            Guid entityGuid = entity.Guid;

            entityRegistry.addEntity (entity);
            entity[name]["i"] = 42;
            entity[name]["f"] = 3.14f;
            entity[name]["s"] = "foobar";
            entity[name]["b"] = false;

            componentRegistry.upgradeComponent(name, plugin.pluginGuid, layout, 2, testUpgrader);
            entityRegistry.OnEntityRemoved -= plugin.onEntityRemoved;
            entityRegistry.removeEntity (entityGuid);

            plugin.retrieveEntitiesFromDatabase ();

            ISet<Guid> guidsInRegistry = entityRegistry.getAllGUIDs ();
            Assert.IsTrue(guidsInRegistry.Contains(entityGuid));

            Entity retrievedEntity = entityRegistry.getEntity (entityGuid);

            Assert.AreEqual(retrievedEntity[name]["i"], 3);
            Assert.AreEqual(retrievedEntity[name]["f"], 42);
            Assert.IsNull(retrievedEntity[name]["s"]);
            Assert.AreEqual(retrievedEntity[name]["b"], false);

        }

        public static void testUpgrader(Component oldComponent, ref Component newComponent) {
            newComponent["f"] = (float)(int)oldComponent["i"];
            newComponent["i"] = (int)(float)oldComponent["f"];
            newComponent["b"] = oldComponent["b"];
        }
    }
}

