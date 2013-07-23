
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
            var g1 = registry.addEntity(e1);
            Assert.NotNull(registry.getEntityByGuid(g1));
        }

        [Test()]
        public void shouldRemoveEntities() {
            var e1 = new Entity();
            var g1 = registry.addEntity(e1);
            Assert.True(registry.removeEntity(g1));
            Assert.Null(registry.getEntityByGuid(g1));
        }

        [Test()]
        public void shouldReturnAllGUIDs() {
            var e1 = new Entity();
            var e2 = new Entity();
            var e3 = new Entity();
            var g1 = registry.addEntity(e1);
            var g2 = registry.addEntity(e2);
            var g3 = registry.addEntity(e3);

            var allGUIDs = registry.getAllGUIDs();

            Assert.Contains(g1, allGUIDs);
            Assert.Contains(g2, allGUIDs);
            Assert.Contains(g3, allGUIDs);
        }
    }
}

