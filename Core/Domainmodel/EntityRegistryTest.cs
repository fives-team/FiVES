
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
            registry.AddEntity(e1);
            Assert.NotNull(registry.GetEntity(e1.Guid));
        }

        [Test()]
        public void shouldRemoveEntities() {
            var e1 = new Entity();
            registry.AddEntity(e1);
            Assert.NotNull(registry.GetEntity(e1.Guid));
            Assert.True(registry.RemoveEntity(e1));
            Assert.Null(registry.GetEntity(e1.Guid));
        }

        [Test()]
        public void shouldRemoveEntitiesByGuid() {
            var e1 = new Entity();
            registry.AddEntity(e1);
            Assert.NotNull(registry.GetEntity(e1.Guid));
            Assert.True(registry.RemoveEntity(e1.Guid));
            Assert.Null(registry.GetEntity(e1.Guid));
        }

        [Test()]
        public void shouldReturnAllGUIDs() {
            var e1 = new Entity();
            var e2 = new Entity();
            var e3 = new Entity();
            registry.AddEntity(e1);
            registry.AddEntity(e2);
            registry.AddEntity(e3);

            var allGUIDs = registry.GetAllGUIDs();

            Assert.True(allGUIDs.Contains(e1.Guid));
            Assert.True(allGUIDs.Contains(e2.Guid));
            Assert.True(allGUIDs.Contains(e3.Guid));
        }
    }
}

