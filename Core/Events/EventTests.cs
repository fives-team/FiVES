using System;
using NUnit.Framework;
using FIVES;

namespace Events
{
    [TestFixture()]
    public class EventTests
    {
        public EventTests ()
        {
        }

        [Test()]
        public void shouldRaiseNewEntityEventWithCorrectGuid()
        {
            Entity newEntity = new Entity ();
            bool wasRaised = false;
            EntityRegistry.Instance.OnEntityAdded += delegate(object sender, EntityAddedOrRemovedEventArgs e) {
                wasRaised = true;
                Assert.AreEqual(newEntity.Guid, e.elementId);
            };
            EntityRegistry.Instance.addEntity (newEntity);
            Assert.IsTrue(wasRaised);
        }

        [Test()]
        public void shouldRaiseEntityRemovedWithCorrectGuid()
        {
            Entity newEntity = new Entity ();
            bool wasRaised = false;
            EntityRegistry.Instance.OnEntityRemoved += delegate(object sender, EntityAddedOrRemovedEventArgs e) {
                wasRaised = true;
                Assert.AreEqual(newEntity.Guid, e.elementId);
           };
            EntityRegistry.Instance.addEntity (newEntity);
            EntityRegistry.Instance.removeEntity (newEntity.Guid);
            Assert.IsTrue (wasRaised);
        }

        [Test()]
        public void shouldRaiseAttributeChanged()
        {
            ComponentLayout layout = new ComponentLayout ();
            layout.attributes ["a"] = AttributeType.INT;
            ComponentRegistry.Instance.defineComponent ("MyComponent", new Guid (), layout);
            Entity newEntity = new Entity ();
            bool wasRaised = false;

            object valueToSet = 42;
            newEntity["MyComponent"].OnAttributeChanged += delegate (object sender, AttributeChangedEventArgs e){
                wasRaised = true;
                Assert.AreEqual(e.attributeName, "a");
                Assert.AreEqual(e.value, valueToSet);
           };
            newEntity ["MyComponent"].setIntAttribute ("a", (int)valueToSet);
            Assert.IsTrue (wasRaised);
        }

        [Test()]
        public void shouldRaiseAttributeInComponentChanged()
        {
            ComponentLayout layout = new ComponentLayout ();
            layout.attributes ["a"] = AttributeType.INT;
            ComponentRegistry.Instance.defineComponent ("MyComponent", new Guid (), layout);
            Entity newEntity = new Entity ();
            bool wasRaised = false;

            object valueToSet = 42;
            newEntity.OnAttributeInComponentChanged += delegate(object sender, AttributeInComponentEventArgs e) {
                wasRaised = true;
                Assert.AreEqual(e.componentName, "MyComponent");
                Assert.AreEqual(e.attributeName, "a");
                Assert.AreEqual(e.newValue, valueToSet);
           };

            newEntity ["MyComponent"].setIntAttribute ("a", (int)valueToSet);
            Assert.IsTrue (wasRaised);
        }
    }
}

