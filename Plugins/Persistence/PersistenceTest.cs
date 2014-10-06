// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
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
    [TestFixture()]
    public class PersistenceTest
    {
        IComponentRegistry componentRegistry;
        EntityCollection entityRegistry;
        Configuration cfg;
        //NHibernate.ISessionFactory sessionFactory;
        PersistencePlugin plugin;

        public PersistenceTest ()
        {

        }

        [SetUp()]
        public void SetUpDatabaseTest() {
            entityRegistry = World.Instance;
            componentRegistry = ComponentRegistry.Instance;
        }

//        [Test()]
//        public void FirstShouldSetupDatabase()
//        {
//            cfg = new Configuration ();
//            cfg.Configure ();
//
//            //sessionFactory = cfg.BuildSessionFactory ();
//
//            cfg.AddAssembly (typeof(Entity).Assembly);
//            new SchemaExport (cfg).Execute (true, true, false);
//        }
//
//        [Test()]
//        public void ShouldStoreAndRetrieveComponent()
//        {
//            ComponentDefinition myComponent = new ComponentDefinition("myComponent");
//            myComponent.AddAttribute<int>("IntAttribute");
//            myComponent.AddAttribute<string>("StringAttribute");
//            componentRegistry.Register(myComponent);
//
//            if (plugin == null) {
//                plugin = new PersistencePlugin ();
//                plugin.Initialize ();
//            }
//
//            Entity entity = new Entity();
//
//            World.Instance.Add(entity);
//            entity["myComponent"]["IntAttribute"].Suggest(42);
//            entity["myComponent"]["StringAttribute"].Suggest("Hello World!");
//
//            // De-Activate on-remove event handler, as for tests, we only want to remove the entity from the local registry, not from the
//            // persistence storage
//            World.Instance.RemovedEntity -= plugin.OnEntityRemoved;
//            World.Instance.Remove(entity);
//
//            plugin.RetrieveEntitiesFromDatabase ();
//
//            Entity storedEntity = World.Instance.FindEntity(entity.Guid.ToString());
//            Assert.AreEqual(42, storedEntity["myComponent"]["IntAttribute"].Value);
//            Assert.AreEqual("Hello World!", storedEntity["myComponent"]["StringAttribute"].Value);
//        }
//
//        [Test()]
//        public void ShouldStoreAndRetrieveEntities()
//        {
//            Entity entity = new Entity();
//
//            if (plugin == null) {
//                plugin = new PersistencePlugin ();
//                plugin.Initialize ();
//            }
//
//            plugin.AddEntityToPersisted(entity);
//            plugin.RetrieveEntitiesFromDatabase ();
//            Assert.True(World.Instance.Contains(entity));
//        }
//
//        [Test()]
//        public void ShouldDeleteEntity()
//        {
//            Entity entity = new Entity();
//
//            if (plugin == null) {
//                plugin = new PersistencePlugin ();
//                plugin.Initialize ();
//            }
//
//            World.Instance.Add (entity);
//            World.Instance.Remove (entity);
//
//            if (!plugin.GlobalSession.IsOpen)
//                plugin.GlobalSession = plugin.SessionFactory.OpenSession();
//            plugin.RetrieveEntitiesFromDatabase ();
//
//            Assert.False(entityRegistry.Contains(entity));
//        }

        public static void TestUpgrader(Component oldComponent, Component newComponent) {
            var newF = (float)(int)oldComponent["i"].Value;
            newComponent["f"].Suggest(newF);

            var newI = (int)(float)oldComponent["f"].Value;
            newComponent["i"].Suggest(newI);

            newComponent["b"].Suggest(oldComponent["b"]);
        }
    }
}

