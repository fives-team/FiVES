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
            layout.AddAttribute<int>("i");
            layout.AddAttribute<float> ("f");
            layout.AddAttribute<string> ("s");
            layout.AddAttribute<bool>("b");
        }

        public static void TestUpgrader(Component oldComponent, ref Component newComponent) {
            newComponent["f"] = (float)(int)oldComponent["i"];
            newComponent["i"] = (int)(float)oldComponent["f"];
            newComponent["b"] = oldComponent["b"];
        }

        [SetUp()]
        public void Init() {
            mockEntityRegistry = new Mock<IEntityRegistry>();
            componentRegistry = new ComponentRegistry(mockEntityRegistry.Object);
        }

        [Test()]
        public void ShouldDefineComponentsWithoutException()
        {
            componentRegistry.DefineComponent(name, Guid.NewGuid(), layout);
        }

        [Test()]
        public void ShouldNotThrowExceptionWhenRedefiningSameComponent()
        {
            Guid owner = Guid.NewGuid();
            componentRegistry.DefineComponent(name, owner, layout);
            componentRegistry.DefineComponent(name, owner, layout);
        }

        [Test()]
        public void ShouldThrowExceptionWhenRedefiningComponentWithDifferentLayout()
        {
            Guid owner = Guid.NewGuid();

            ComponentLayout otherLayout = new ComponentLayout();
            otherLayout.AddAttribute<bool>("a");

            componentRegistry.DefineComponent(name, owner, layout);
            Assert.Throws<ComponentAlreadyDefinedException>(
                delegate() { componentRegistry.DefineComponent(name, owner, otherLayout); } );
        }

        [Test()]
        public void ShouldThrowExceptionWhenRedefiningComponentWithDifferentOwner()
        {
            componentRegistry.DefineComponent(name, Guid.NewGuid(), layout);
            Assert.Throws<ComponentAlreadyDefinedException>(
                delegate() { componentRegistry.DefineComponent(name, Guid.NewGuid(), layout); } );

        }

        [Test()]
        public void ShouldFailToConstructUndefinedComponent() {
            Assert.Throws<ComponentIsNotDefinedException>(
                delegate() { componentRegistry.GetComponentInstance("foobar"); } );
        }

        [Test()]
        public void ShouldCreateDefinedLayoutAttributesInNewComponents()
        {
            componentRegistry.DefineComponent(name, Guid.NewGuid(), layout);
            Component c = componentRegistry.GetComponentInstance(name);
            Assert.AreEqual(c["i"], default(int));
            Assert.AreEqual(c["f"], default(float));
            Assert.AreEqual(c["s"], default(string));
            Assert.AreEqual(c["b"], default(bool));
        }

        [Test()]
        public void ShouldNotCreateUndefinedAttributesInNewComponents()
        {
            componentRegistry.DefineComponent(name, Guid.NewGuid(), layout);
            Component c = componentRegistry.GetComponentInstance(name);
            Assert.Throws<AttributeIsNotDefinedException>(
                delegate() { object result = c["foobar"]; } );
        }

        [Test]
        public void ShouldFailToUpgradeUndefinedComponent()
        {
            Assert.Throws<ComponentIsNotDefinedException>(delegate() {
                componentRegistry.UpgradeComponent(name, Guid.NewGuid(), layout, 5, TestUpgrader);
            });
        }

        [Test]
        public void ShouldFailToUpgradeToInvalidVersion()
        {
            Guid owner = Guid.NewGuid();
            componentRegistry.DefineComponent(name, owner, layout);
            Assert.Throws<InvalidUpgradeVersion>(delegate() {
                componentRegistry.UpgradeComponent(name, owner, layout, -1, TestUpgrader);
            });

            Assert.Throws<InvalidUpgradeVersion>(delegate() {
                componentRegistry.UpgradeComponent(name, owner, layout, 1, TestUpgrader);
            });

            Assert.Throws<InvalidUpgradeVersion>(delegate() {
                componentRegistry.UpgradeComponent(name, owner, layout, 0, TestUpgrader);
            });
        }

        [Test]
        public void ShouldFailToUpgradeToADifferentOwner()
        {
            componentRegistry.DefineComponent(name, Guid.NewGuid(), layout);
            Assert.Throws<InvalidUpgradeOwner>(delegate() {
                componentRegistry.UpgradeComponent(name, Guid.NewGuid(), layout, 2, TestUpgrader);
            });
        }

        [Test]
        public void ShouldFailToUpgradeWithoutAnUpgrader()
        {
            Guid owner = Guid.NewGuid();
            componentRegistry.DefineComponent(name, owner, layout);
            Assert.Throws<ArgumentNullException>(delegate() {
                componentRegistry.UpgradeComponent(name, owner, layout, 2, null);
            });
        }

        [Test]
        public void ShouldUpgradeComponentsCorrectly()
        {
            Guid owner = Guid.NewGuid();
            componentRegistry.DefineComponent(name, owner, layout);

            var entity = new Entity(componentRegistry);

            mockEntityRegistry.Setup(r => r.GetAllGUIDs()).Returns(new HashSet<Guid>{entity.Guid});
            mockEntityRegistry.Setup(r => r.GetEntity(entity.Guid)).Returns(entity);

            entity[name]["i"] = 42;
            entity[name]["f"] = 3.14f;
            entity[name]["s"] = "foobar";
            entity[name]["b"] = false;

            componentRegistry.UpgradeComponent(name, owner, layout, 2, TestUpgrader);

            mockEntityRegistry.Verify(r => r.GetAllGUIDs(), Times.Once());
            mockEntityRegistry.Verify(r => r.GetEntity(entity.Guid), Times.Once());

            Assert.AreEqual(entity[name]["i"], 3);
            Assert.AreEqual(entity[name]["f"], 42);
            Assert.IsNull(entity[name]["s"]);
            Assert.AreEqual(entity[name]["b"], false);
        }

        public interface UpgradeEventTester {
            void HandleStarted(Object sender, ComponentLayoutUpgradeStartedOrFinishedEventArgs e);
            void HandleFinished(Object sender, ComponentLayoutUpgradeStartedOrFinishedEventArgs e);
            void HandleUpgraded(Object sender, EntityComponentUpgradedEventArgs e);
        }

        [Test]
        public void ShouldGenerateUpgradeEventsInOrder()
        {
            // These flags and following callbacks are used to verify that appropriate events are generated in order.
            // For example, upgraded flag is only set when it happens after a started event, but before finished. Later
            // Verify calls also verify that each event is only triggered once.
            bool started = false;
            bool upgraded = false;
            bool finished = false;

            Mock<UpgradeEventTester> tester = new Mock<UpgradeEventTester>();
            tester.Setup(t => t.HandleStarted(It.IsAny<Object>(),
                It.IsAny<ComponentLayoutUpgradeStartedOrFinishedEventArgs>()))
                .Callback(delegate() { started = true; });
            tester.Setup(t => t.HandleUpgraded(It.IsAny<Object>(), It.IsAny<EntityComponentUpgradedEventArgs>()))
                .Callback(delegate() { if (started && !finished) upgraded = true; });
            tester.Setup(t => t.HandleFinished(It.IsAny<Object>(),
                It.IsAny<ComponentLayoutUpgradeStartedOrFinishedEventArgs>()))
                .Callback(delegate() { if (started) finished = true; });

            Guid owner = Guid.NewGuid();
            componentRegistry.DefineComponent(name, owner, layout);

            componentRegistry.OnComponentLayoutUpgradeStarted += tester.Object.HandleStarted;
            componentRegistry.OnComponentLayoutUpgradeFinished += tester.Object.HandleFinished;
            componentRegistry.OnEntityComponentUpgraded += tester.Object.HandleUpgraded;

            var entity = new Entity(componentRegistry);

            mockEntityRegistry.Setup(r => r.GetAllGUIDs()).Returns(new HashSet<Guid>{entity.Guid});
            mockEntityRegistry.Setup(r => r.GetEntity(entity.Guid)).Returns(entity);

            entity[name]["i"] = 42;
            entity[name]["f"] = 3.14f;
            entity[name]["s"] = "foobar";
            entity[name]["b"] = false;

            componentRegistry.UpgradeComponent(name, owner, layout, 2, TestUpgrader);

            tester.Verify(t => t.HandleStarted(componentRegistry, 
                It.Is<ComponentLayoutUpgradeStartedOrFinishedEventArgs>(a => a.componentName == name)), Times.Once());
            tester.Verify(t => t.HandleFinished(componentRegistry, 
                It.Is<ComponentLayoutUpgradeStartedOrFinishedEventArgs>(a => a.componentName == name)), Times.Once());
            tester.Verify(t => t.HandleUpgraded(componentRegistry, 
                It.Is<EntityComponentUpgradedEventArgs>(a => a.componentName == name && a.entity == entity)), 
                Times.Once());

            Assert.IsTrue(started);
            Assert.IsTrue(upgraded);
            Assert.IsTrue(finished);
        }

        [Test]
        public void ShouldOnlyUpgradeOutdatedComponents()
        {
            Guid owner = Guid.NewGuid();
            componentRegistry.DefineComponent(name, owner, layout);

            var entity = new Entity(componentRegistry);

            mockEntityRegistry.Setup(r => r.GetAllGUIDs()).Returns(new HashSet<Guid>{entity.Guid});
            mockEntityRegistry.Setup(r => r.GetEntity(entity.Guid)).Returns(entity);

            entity[name]["i"] = 42;
            entity[name]["f"] = 3.14f;
            entity[name]["s"] = "foobar";
            entity[name]["b"] = false;
            entity[name].Version = 2;

            componentRegistry.UpgradeComponent(name, owner, layout, 2, TestUpgrader);

            mockEntityRegistry.Verify(r => r.GetAllGUIDs(), Times.Once());
            mockEntityRegistry.Verify(r => r.GetEntity(entity.Guid), Times.Once());

            Assert.AreEqual(entity[name]["i"], 42);
            Assert.AreEqual(entity[name]["f"], 3.14f);
            Assert.AreEqual(entity[name]["s"], "foobar");
            Assert.AreEqual(entity[name]["b"], false);
        }

        [Test()]
        public void ShouldReturnRegisteredComponentLayout()
        {
            componentRegistry.DefineComponent(name, Guid.NewGuid(), layout);
            Assert.AreEqual(componentRegistry.GetComponentLayout(name), layout);
        }
    }
}

