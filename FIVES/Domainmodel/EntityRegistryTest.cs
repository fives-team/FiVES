
using System;
using NUnit.Framework;

namespace FIVES
{
    [TestFixture()]
    public class EntityRegistryTest
    {
        [Test()]
        public void ShouldAddEntitites() {
            var e1 = new Entity();
            var g1 = EntityRegistry.AddEntity(e1);
            Assert.NotNull(EntityRegistry.GetEntityByGuid(g1));
        }

        [Test()]
        public void ShouldRemoveEntities() {
            var e1 = new Entity();
            var g1 = EntityRegistry.AddEntity(e1);
            Assert.True(EntityRegistry.RemoveEntity(g1));
            Assert.Null(EntityRegistry.GetEntityByGuid(g1));
        }

        [Test()]
        public void ShouldReturnAllGUIDs() {
            var e1 = new Entity();
            var e2 = new Entity();
            var e3 = new Entity();
            var g1 = EntityRegistry.AddEntity(e1);
            var g2 = EntityRegistry.AddEntity(e2);
            var g3 = EntityRegistry.AddEntity(e3);

            var allGUIDs = EntityRegistry.GetAllGUIDs();

            Assert.Contains(g1, allGUIDs);
            Assert.Contains(g2, allGUIDs);
            Assert.Contains(g3, allGUIDs);
        }
    }
}

