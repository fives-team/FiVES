
using System;
using NUnit.Framework;

namespace FIVES
{
    [TestFixture()]
    public class EntityRegistryTest
    {
        [Test()]
        public void shouldAddEntitites() {
            var e1 = new Entity();
            var g1 = EntityRegistry.addEntity(e1);
            Assert.NotNull(EntityRegistry.getEntityByGuid(g1));
        }

        [Test()]
        public void shouldRemoveEntities() {
            var e1 = new Entity();
            var g1 = EntityRegistry.addEntity(e1);
            Assert.True(EntityRegistry.removeEntity(g1));
            Assert.Null(EntityRegistry.getEntityByGuid(g1));
        }

        [Test()]
        public void shouldReturnAllGUIDs() {
            var e1 = new Entity();
            var e2 = new Entity();
            var e3 = new Entity();
            var g1 = EntityRegistry.addEntity(e1);
            var g2 = EntityRegistry.addEntity(e2);
            var g3 = EntityRegistry.addEntity(e3);

            var allGUIDs = EntityRegistry.getAllGUIDs();

            Assert.Contains(g1, allGUIDs);
            Assert.Contains(g2, allGUIDs);
            Assert.Contains(g3, allGUIDs);
        }
    }
}

