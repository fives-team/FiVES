using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
{
    [TestFixture()]
    public class EntityTest
    {
        public interface IMockHandlers 
        {
            void CreatedComponent(object sender, ComponentEventArgs e);
            void ChangedAttribute(object sender, ChangedAttributeEventArgs e);
        }

        Entity entity;
        Mock<IMockHandlers> mockHandlers;
        Mock<IComponentRegistry> mockComponentRegistry;

        [SetUp()]
        public void Init()
        {
            // TODO: mock ComponentDefinition
            ComponentDefinition test = new ComponentDefinition("test");
            test.AddAttribute<int>("a", 42);

            mockComponentRegistry = new Mock<IComponentRegistry>();
            mockComponentRegistry.Setup(r => r.FindComponentDefinition("test")).Returns(test);

            entity = new Entity();
            entity.componentRegistry = mockComponentRegistry.Object;

            mockHandlers = new Mock<IMockHandlers>();
        }

        [Test()]
        public void ShouldInitializeGuid()
        {
            Assert.IsNotNull(entity.Guid);
            Assert.AreNotEqual(Guid.Empty, entity.Guid);
        }

        [Test()]
        public void ShouldCreateComponentsWhenAccessedAndCorrectVerifyIfEntityContainsAComponent()
        {
            Assert.IsFalse(entity.ContainsComponent("test"));
            entity["test"]["a"] = 24;
            Assert.IsTrue(entity.ContainsComponent("test"));
        }

        [Test()]
        [ExpectedException(typeof(ComponentAccessException))]
        public void ShouldFailToCreateUnregisteredComponents()
        {
            entity["unregistered-component"]["a"] = 24;
        }

        [Test()]
        public void ShouldReturnCollectionOfComponents()
        {
            entity["test"]["a"] = 24;

            Assert.AreEqual(1, entity.Components.Count);
            var enumerator = entity.Components.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual("test", enumerator.Current.Name);
            Assert.IsFalse(enumerator.MoveNext());
        }

        [Test()]
        public void ShouldFireCreatedComponentEvents()
        {
            entity.CreatedComponent += mockHandlers.Object.CreatedComponent;

            entity["test"]["a"] = 24;

            mockHandlers.Verify(h => h.CreatedComponent(It.IsAny<object>(), 
                It.IsAny<ComponentEventArgs>()), Times.Once());
        }

        [Test()]
        public void ShouldFireChangedAttributeEvents()
        {
            entity.ChangedAttribute += mockHandlers.Object.ChangedAttribute;

            entity["test"]["a"] = 24;

            mockHandlers.Verify(h => h.ChangedAttribute(It.IsAny<object>(),
                It.IsAny<ChangedAttributeEventArgs>()), Times.Once());
        }
    }
}
