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
        public void ShouldAddOneChildren()
        {
            Entity parentEntity = new Entity ();
            var EntityToAdd = new Entity ();
            Assert.True(parentEntity.AddChildNode (EntityToAdd));
            Assert.False(parentEntity.AddChildNode (EntityToAdd));
            Assert.IsTrue (parentEntity.GetAllChildren () [0] == EntityToAdd);
            Assert.IsTrue (parentEntity.GetAllChildren ().Count == 1);
        }

        [Test()]
        public void ShouldCorrectlyAssignParent()
        {
            Entity parentEntity = new Entity ();
            Entity childEntity = new Entity ();
            parentEntity.AddChildNode (childEntity);
            Assert.IsTrue (childEntity.Parent == parentEntity);
        }

        [Test()]
        public void ShouldCorrectlyRemoveChild()
        {
            Entity parentEntity = new Entity ();
            Entity childEntity = new Entity ();
            parentEntity.AddChildNode (childEntity);

            Assert.True (parentEntity.RemoveChild (childEntity));
            Assert.IsEmpty (parentEntity.GetAllChildren ());
            Assert.IsNull (childEntity.Parent);
        }

        [Test()]
        public void ShouldCorrectlyChangeParenthood()
        {
            Entity parentEntity_1 = new Entity ();
            Entity parentEntity_2 = new Entity ();
            Entity childEntity = new Entity ();
            parentEntity_1.AddChildNode (childEntity);
            parentEntity_2.AddChildNode (childEntity);

            Assert.IsEmpty (parentEntity_1.GetAllChildren ());
            Assert.Contains (childEntity, parentEntity_2.GetAllChildren ());
            Assert.IsTrue (childEntity.Parent == parentEntity_2);
        }
        [Test()]
        public void ShouldCorrectlyRetrieveFirstAndLast()
        {
            Entity parentEntity = new Entity ();
            Entity firstChildEntity = new Entity ();
            Entity lastChildEntity = new Entity ();

            parentEntity.AddChildNode (firstChildEntity);
            parentEntity.AddChildNode (lastChildEntity);

            Assert.IsTrue (parentEntity.GetFirstChild () == firstChildEntity);
            Assert.IsTrue (parentEntity.GetLastChild () == lastChildEntity);
        }

        [Test()]
        public void ShouldCreateRegisteredComponentAutomatically()
        {
            // Define new component type "myComponent" with one int attribute "attr".
            ComponentRegistry myRegistry = new ComponentRegistry();
            ComponentLayout layout = new ComponentLayout();
            layout.AddAttribute<int> ("attr");
            myRegistry.DefineComponent("myComponent", Guid.NewGuid(), layout);

            Entity entity = new Entity(myRegistry);
            entity["myComponent"]["attr"] = 42;
        }

        [Test()]
        public void ShouldThrowExceptionWhenAccessingUnregisteredComponent()
        {
            Entity entity = new Entity();
            Assert.Throws<ComponentIsNotDefinedException>(delegate() {
                Component component = entity["myComponent"];
            });
        }

        [Test()]
        public void ShouldThrowExceptionWhenCreatingUnregisteredComponent()
        {
            Entity entity = new Entity();
            Assert.Throws<ComponentIsNotDefinedException>(delegate() {
                Component component = entity["myComponent"];
            });
        }

        [Test()]
        public void ShouldAddAndRemoveComponent()
        {
            // Define new component type "myComponent" with one int attribute "attr".
            ComponentRegistry myRegistry = new ComponentRegistry();
            ComponentLayout layout = new ComponentLayout();
            layout.AddAttribute<int> ("attr");
            myRegistry.DefineComponent("myComponent", Guid.NewGuid(), layout);

            Entity entity = new Entity(myRegistry);
            entity["myComponent"]["attr"] =  42;
            Assert.IsTrue(entity.HasComponent("myComponent"));
            entity.RemoveComponent("myComponent");
            Assert.IsFalse(entity.HasComponent("myComponent"));
        }
    }
}

