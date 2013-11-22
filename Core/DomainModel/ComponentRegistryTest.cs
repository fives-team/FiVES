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
        // TODO: mock ComponentDefinition
        ComponentRegistry registry;
        ComponentDefinition definition;

        [SetUp()]
        public void Init()
        {
            registry = new ComponentRegistry();
            definition = new ComponentDefinition("test2");
            definition.AddAttribute<int>("a", 42);
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
        public void ShouldFailToRegisterComponentTwice()
        {
            registry.Register(definition);
            registry.Register(definition);
        }
    }
}
