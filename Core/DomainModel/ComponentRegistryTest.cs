using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
{
    [TestFixture()]
    public class ComponentRegistryTest
    {
        public interface IMockHandlers 
        {
            void UpgradedComponent(object sender, ComponentEventArgs e);
        }

        // TODO: mock ComponentDefinition
        ComponentRegistry registry;
        ComponentDefinition definition;
        ComponentDefinition definition2;
        Mock<IMockHandlers> mockHandlers;

        [SetUp()]
        public void Init()
        {
            registry = new ComponentRegistry();
            mockHandlers = new Mock<IMockHandlers>();

            definition = new ComponentDefinition("test2");
            definition.AddAttribute<int>("a", 42);

            definition2 = new ComponentDefinition("test2", 2);
        }

        [Test()]
        public void ShouldRetrieveCollectionOfRegisteredComponents()  // also checks for Register
        {
            registry.Register(definition);
            Assert.AreEqual(1, registry.RegisteredComponents.Count);
            var enumerator = registry.RegisteredComponents.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual("test2", enumerator.Current.Name);
            Assert.IsFalse(enumerator.MoveNext());
        }

        [Test()]
        [ExpectedException(typeof(ComponentRegistrationException))]
        public void ShouldFailTegisterComponentTwice()
        {
            registry.Register(definition);
            registry.Register(definition2);
        }

        [Test()]
        [ExpectedException(typeof(ComponentUpgradeException))]
        public void ShouldFailToUpgradeNonExistingComponent()
        {
            registry.Upgrade(definition2, (oldComponent, newComponent) => { });
        }

        [Test()]
        [ExpectedException(typeof(ComponentUpgradeException))]
        public void ShouldFailToUpgradeComponentWhenNewVersionIsTooLarge()
        {
            var definition3 = new ComponentDefinition("test2", 3);
            registry.Register(definition);
            registry.Upgrade(definition3, (oldComponent, newComponent) => { });
        }

        [Test()]
        [ExpectedException(typeof(ComponentUpgradeException))]
        public void ShouldFailToUpgradeComponentWhenNewVersionIsTheSame()
        {
            registry.Register(definition);
            registry.Upgrade(definition, (oldComponent, newComponent) => { });
        }

        [Test()]
        public void ShouldUpdateRegisteredComponentAfterUpgrade()  // also checks FindComponentDefinition
        {
            registry.Register(definition);
            Assert.AreEqual(1, registry.FindComponentDefinition("test2").Version);
            registry.Upgrade(definition2, (oldComponent, newComponent) => { });
            Assert.AreEqual(2, registry.FindComponentDefinition("test2").Version);
        }

        [Test()]
        public void ShouldFireUpgradedComponentEvents()
        {
            // TODO: mock Entity and World
            var entity = new Entity();
            World.Instance.Add(entity);
            registry.Register(definition);
            ComponentRegistry.Instance.Register(definition);  // need to register in global registry too
            entity["test2"]["a"] = 24;

            registry.UpgradedComponent += mockHandlers.Object.UpgradedComponent;
            registry.Upgrade(definition2, (oldComponent, newComponent) => { });

            mockHandlers.Verify(h => h.UpgradedComponent(It.IsAny<object>(), 
                It.IsAny<ComponentEventArgs>()), Times.Once());
        }
    }
}
