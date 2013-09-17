using System;
using NUnit.Framework;
using FIVES;

namespace Events
{
    [TestFixture()]
    public class EventTests
    {
        private static bool eventWasRaised = false;

        public EventTests ()
        {
        }

        [SetUp()]
        public void SetUpTests()
        {
            eventWasRaised = false;
        }

        [Test()]
        public void ShouldRaiseNewEntityEventWithCorrectGuid()
        {
            Entity newEntity = new Entity ();
            bool wasRaised = false;
            EntityRegistry.Instance.OnEntityAdded += delegate(object sender, EntityAddedOrRemovedEventArgs e) {
                wasRaised = true;
                Assert.AreEqual(newEntity.Guid, e.elementId);
            };
            EntityRegistry.Instance.AddEntity (newEntity);
            Assert.IsTrue(wasRaised);
        }

        [Test()]
        public void ShouldRaiseEntityRemovedWithCorrectGuid()
        {
            Entity newEntity = new Entity ();
            bool wasRaised = false;
            EntityRegistry.Instance.OnEntityRemoved += delegate(object sender, EntityAddedOrRemovedEventArgs e) {
                wasRaised = true;
                Assert.AreEqual(newEntity.Guid, e.elementId);
           };
            EntityRegistry.Instance.AddEntity (newEntity);
            EntityRegistry.Instance.RemoveEntity (newEntity.Guid);
            Assert.IsTrue (wasRaised);
        }

        [Test()]
        public void ShouldRaiseAttributeChanged()
        {
            ComponentLayout layout = new ComponentLayout ();
            layout.AddAttribute<int> ("a");
            ComponentRegistry.Instance.DefineComponent ("MyComponent", new Guid (), layout);
            Entity newEntity = new Entity ();

            Component.AttributeChanged attributeChanged = new Component.AttributeChanged (AttributeChangedHandler);
            newEntity["MyComponent"].OnAttributeChanged += attributeChanged;            newEntity["MyComponent"]["a"] = 42;
            Assert.IsTrue (eventWasRaised);
        }

        private void AttributeChangedHandler(Object sender, AttributeChangedEventArgs e) {
            eventWasRaised = true;
            Assert.AreEqual(e.attributeName, "a");
            Assert.AreEqual(e.value, 42);
        }


        [Test()]
        public void ShouldRaiseComponentCreated()
        {
            ComponentLayout layout = new ComponentLayout ();
            layout.AddAttribute<int> ("a");
            ComponentRegistry.Instance.DefineComponent ("MyComponent", new Guid (), layout);
            Entity newEntity = new Entity ();
            newEntity.OnComponentCreated += new Entity.ComponentCreated(ComponentCreatedEventHandler);
            newEntity["MyComponent"]["a"] = 5;
            Assert.IsTrue (eventWasRaised);
        }

        private void ComponentCreatedEventHandler(Object sender, ComponentCreatedEventArgs e)
        {
           if (e.newComponentName == "MyComponent")
                eventWasRaised = true;
        }

        [Test()]
        public void ShouldRaiseAttributeInComponentChanged()
        {
            ComponentLayout layout = new ComponentLayout ();
            layout.AddAttribute<int> ("a");
            ComponentRegistry.Instance.DefineComponent ("MyComponent", new Guid (), layout);
            Entity newEntity = new Entity ();

            Entity.AttributeInComponentChanged attributeInComponentChanged = new Entity.AttributeInComponentChanged (AttributeInComponentChangedHandler);
            newEntity.OnAttributeInComponentChanged += attributeInComponentChanged;                         
            newEntity["MyComponent"]["a"] = 42;
            Assert.IsTrue (eventWasRaised);
        }

        private void AttributeInComponentChangedHandler(Object sender, AttributeInComponentEventArgs e) {
            eventWasRaised = true;
            Assert.AreEqual(e.componentName, "MyComponent");
            Assert.AreEqual(e.attributeName, "a");
            Assert.AreEqual(e.newValue, 42);
        }
    }
}

