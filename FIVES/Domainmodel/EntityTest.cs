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
			Assert.True(parentEntity.addChildNode (EntityToAdd));
			Assert.False(parentEntity.addChildNode (EntityToAdd));
			Assert.IsTrue (parentEntity.getAllChildren () [0] == EntityToAdd);
			Assert.IsTrue (parentEntity.getAllChildren ().Count == 1);
		}

		[Test()]
		public void ShouldCorrectlyAssignParent()
		{
			Entity parentEntity = new Entity ();
			Entity childEntity = new Entity ();
			parentEntity.addChildNode (childEntity);
			Assert.IsTrue (childEntity.parent == parentEntity);
		}

		[Test()]
		public void ShouldCorrectlyRetrieveFirstAndLast()
		{
			Entity parentEntity = new Entity ();
			Entity firstChildEntity = new Entity ();
			Entity lastChildEntity = new Entity ();

			parentEntity.addChildNode (firstChildEntity);
			parentEntity.addChildNode (lastChildEntity);

			Assert.IsTrue (parentEntity.getFirstChild () == firstChildEntity);
			Assert.IsTrue (parentEntity.getLastChild () == lastChildEntity);
		}
	}
}

