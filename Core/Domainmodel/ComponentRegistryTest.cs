using NUnit.Framework;
using System;
using Moq;
using System.Collections.Generic;

namespace FIVES
{
    [TestFixture()]
    public class ComponentRegistryTest
    {
        ComponentLayout layout = new ComponentLayout();
        Mock<IEntityRegistry> mockEntityRegistry;
        ComponentRegistry componentRegistry;
        string name = "myComponent";

        public ComponentRegistryTest() {
            layout.addAttribute<int>("i");
            layout.addAttribute<float> ("f");
            layout.addAttribute<string> ("s");
            layout.addAttribute<bool>("b");
        }

        public static void testUpgrader(Component oldComponent, ref Component newComponent) {
            newComponent["f"] = (float)(int)oldComponent["i"];
            newComponent["i"] = (int)(float)oldComponent["f"];
            newComponent["b"] = oldComponent["b"];
        }

        [SetUp()]
        public void init() {
            mockEntityRegistry = new Mock<IEntityRegistry>();
            componentRegistry = new ComponentRegistry(mockEntityRegistry.Object);
        }

        [Test()]
        public void shouldDefineComponentsWithoutException()
        {
            componentRegistry.defineComponent(name, Guid.NewGuid(), layout);
        }

        [Test()]
        public void shouldNotThrowExceptionWhenRedefiningSameComponent()
        {
            Guid owner = Guid.NewGuid();
            componentRegistry.defineComponent(name, owner, layout);
            componentRegistry.defineComponent(name, owner, layout);
        }

        [Test()]
        public void shouldThrowExceptionWhenRedefiningComponentWithDifferentLayout()
        {
            Guid owner = Guid.NewGuid();

            ComponentLayout otherLayout = new ComponentLayout();
            otherLayout.addAttribute<bool>("a");

            componentRegistry.defineComponent(name, owner, layout);
            Assert.Throws<ComponentAlreadyDefinedException>(
                delegate() { componentRegistry.defineComponent(name, owner, otherLayout); } );
        }

        [Test()]
        public void shouldThrowExceptionWhenRedefiningComponentWithDifferentOwner()
        {
            componentRegistry.defineComponent(name, Guid.NewGuid(), layout);
            Assert.Throws<ComponentAlreadyDefinedException>(
                delegate() { componentRegistry.defineComponent(name, Guid.NewGuid(), layout); } );

        }

        [Test()]
        public void shouldFailToConstructUndefinedComponent() {
            Assert.Throws<ComponentIsNotDefinedException>(
                delegate() { componentRegistry.getComponentInstance("foobar"); } );
        }

        [Test()]
        public void shouldCreateDefinedLayoutAttributesInNewComponents()
        {
            registry.defineComponent(name, Guid.NewGuid(), layout);
            dynamic c = registry.getComponentInstance(name);
            Assert.AreEqual(c.i, default(int));
            Assert.AreEqual(c.f, default(float));
            Assert.AreEqual(c.s, default(string));
            Assert.AreEqual(c.b, default(bool));
        }

        [Test()]
        public void shouldNotCreateUndefinedAttributesInNewComponents()
        {
            componentRegistry.defineComponent(name, Guid.NewGuid(), layout);
            dynamic c = componentRegistry.getComponentInstance(name);
            Assert.Throws<AttributeIsNotDefinedException>(
                delegate() { object result = c.foobar; } );
        }

        [Test]
        public void shouldFailToUpgradeUndefinedComponent()
        {
            Assert.Throws<ComponentIsNotDefinedException>(delegate() {
                componentRegistry.upgradeComponent(name, Guid.NewGuid(), layout, 5, testUpgrader);
            });
        }

        [Test]
        public void shouldFailToUpgradeToInvalidVersion()
        {
            Guid owner = Guid.NewGuid();
            componentRegistry.defineComponent(name, owner, layout);
            Assert.Throws<InvalidUpgradeVersion>(delegate() {
                componentRegistry.upgradeComponent(name, owner, layout, -1, testUpgrader);
            });

            Assert.Throws<InvalidUpgradeVersion>(delegate() {
                componentRegistry.upgradeComponent(name, owner, layout, 1, testUpgrader);
            });

            Assert.Throws<InvalidUpgradeVersion>(delegate() {
                componentRegistry.upgradeComponent(name, owner, layout, 0, testUpgrader);
            });
        }

        [Test]
        public void shouldFailToUpgradeToADifferentOwner()
        {
            componentRegistry.defineComponent(name, Guid.NewGuid(), layout);
            Assert.Throws<InvalidUpgradeOwner>(delegate() {
                componentRegistry.upgradeComponent(name, Guid.NewGuid(), layout, 2, testUpgrader);
            });
        }

        [Test]
        public void shouldFailToUpgradeWithoutAnUpgrader()
        {
            Guid owner = Guid.NewGuid();
            componentRegistry.defineComponent(name, owner, layout);
            Assert.Throws<ArgumentNullException>(delegate() {
                componentRegistry.upgradeComponent(name, owner, layout, 2, null);
            });
        }

        [Test]
        public void shouldUpgradeComponentsCorrectly()
        {
            Guid owner = Guid.NewGuid();
            componentRegistry.defineComponent(name, owner, layout);

            var entity = new Entity(componentRegistry);

            mockEntityRegistry.Setup(r => r.getAllGUIDs()).Returns(new HashSet<Guid>{entity.Guid});
            mockEntityRegistry.Setup(r => r.getEntity(entity.Guid)).Returns(entity);

            entity[name]["i"] = 42;
            entity[name]["f"] = 3.14f;
            entity[name]["s"] = "foobar";
            entity[name]["b"] = false;

            componentRegistry.upgradeComponent(name, owner, layout, 2, testUpgrader);

            mockEntityRegistry.Verify(r => r.getAllGUIDs(), Times.Once());
            mockEntityRegistry.Verify(r => r.getEntity(entity.Guid), Times.Once());

            Assert.AreEqual(entity[name]["i"], 3);
            Assert.AreEqual(entity[name]["f"], 42);
            Assert.IsNull(entity[name]["s"]);
            Assert.AreEqual(entity[name]["b"], false);
        }
    }
}

