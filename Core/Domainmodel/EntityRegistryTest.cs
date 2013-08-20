
using System;
using NUnit.Framework;

namespace FIVES
{
    [TestFixture()]
    public class EntityRegistryTest
    {
        EntityRegistry registry;

        [SetUp()]
        public void init() {
            registry = new EntityRegistry();
        }

        [Test()]
        public void shouldAddEntitites() {
            var e1 = new Entity();
            registry.addEntity(e1);
            Assert.NotNull(registry.getEntity(e1.Guid));
        }

        [Test()]
        public void shouldRemoveEntities() {
            var e1 = new Entity();
            registry.addEntity(e1);
            Assert.NotNull(registry.getEntity(e1.Guid));
            Assert.True(registry.removeEntity(e1));
            Assert.Null(registry.getEntity(e1.Guid));
        }

        [Test()]
        public void shouldRemoveEntitiesByGuid() {
            var e1 = new Entity();
            registry.addEntity(e1);
            Assert.NotNull(registry.getEntity(e1.Guid));
            Assert.True(registry.removeEntity(e1.Guid));
            Assert.Null(registry.getEntity(e1.Guid));
        }

        [Test()]
        public void shouldReturnAllGUIDs() {
            var e1 = new Entity();
            var e2 = new Entity();
            var e3 = new Entity();
            registry.addEntity(e1);
            registry.addEntity(e2);
            registry.addEntity(e3);

            var allGUIDs = registry.getAllGUIDs();

            Assert.True(allGUIDs.Contains(e1.Guid));
            Assert.True(allGUIDs.Contains(e2.Guid));
            Assert.True(allGUIDs.Contains(e3.Guid));
        }
    }
}

