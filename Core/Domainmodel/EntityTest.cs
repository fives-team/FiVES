using System;
using NUnit.Framework;

namespace FIVES
{
    [TestFixture()]
    public class EntityTest
    {

        public EntityTest ()
        {
        }

        [Test()]
        public void shouldAddOneChildren()
        {
            Entity parentEntity = new Entity ();
            var EntityToAdd = new Entity ();
            Assert.True(parentEntity.addChildNode (EntityToAdd));
            Assert.False(parentEntity.addChildNode (EntityToAdd));
            Assert.IsTrue (parentEntity.getAllChildren () [0] == EntityToAdd);
            Assert.IsTrue (parentEntity.getAllChildren ().Count == 1);
        }

        [Test()]
        public void shouldCorrectlyAssignParent()
        {
            Entity parentEntity = new Entity ();
            Entity childEntity = new Entity ();
            parentEntity.addChildNode (childEntity);
            Assert.IsTrue (childEntity.parent == parentEntity);
        }

        [Test()]
        public void shouldCorrectlyRemoveChild()
        {
            Entity parentEntity = new Entity ();
            Entity childEntity = new Entity ();
            parentEntity.addChildNode (childEntity);

            Assert.True (parentEntity.removeChild (childEntity));
            Assert.IsEmpty (parentEntity.getAllChildren ());
            Assert.IsNull (childEntity.parent);
        }

        [Test()]
        public void shouldCorrectlyChangeParenthood()
        {
            Entity parentEntity_1 = new Entity ();
            Entity parentEntity_2 = new Entity ();
            Entity childEntity = new Entity ();
            parentEntity_1.addChildNode (childEntity);
            parentEntity_2.addChildNode (childEntity);

            Assert.IsEmpty (parentEntity_1.getAllChildren ());
            Assert.Contains (childEntity, parentEntity_2.getAllChildren ());
            Assert.IsTrue (childEntity.parent == parentEntity_2);
        }
        [Test()]
        public void shouldCorrectlyRetrieveFirstAndLast()
        {
            Entity parentEntity = new Entity ();
            Entity firstChildEntity = new Entity ();
            Entity lastChildEntity = new Entity ();

            parentEntity.addChildNode (firstChildEntity);
            parentEntity.addChildNode (lastChildEntity);

            Assert.IsTrue (parentEntity.getFirstChild () == firstChildEntity);
            Assert.IsTrue (parentEntity.getLastChild () == lastChildEntity);
        }

        [Test()]
        public void shouldCreateRegisteredComponent()
        {
            // Define new component type "myComponent" with one int attribute "attr".
            ComponentRegistry myRegistry = new ComponentRegistry();
            ComponentLayout layout = new ComponentLayout();
            layout["attr"] = AttributeType.INT;
            myRegistry.defineComponent("myComponent", Guid.NewGuid(), layout);

            dynamic entity = new Entity(myRegistry);
            entity.myComponent.setIntAttribute("attr", 42);
        }

        [Test()]
        public void shouldThrowExceptionWhenAccessingUnregisteredComponent()
        {
            dynamic entity = new Entity();
            Assert.Throws<ComponentIsNotDefinedException>(delegate() {
                var component = entity.myComponent;
            });
        }

        [Test()]
        public void shouldThrowExceptionWhenCreatingUnregisteredComponent()
        {
            dynamic entity = new Entity();
            Assert.Throws<ComponentIsNotDefinedException>(delegate() {
                var component = entity.myComponent;
            });
        }
    }
}

