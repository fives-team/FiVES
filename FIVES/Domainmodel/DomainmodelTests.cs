using NUnit.Framework;
using System;

namespace FIVES
{
	[TestFixture()]
	public class DomainmodelTests
	{
		private Entity testEntity;
		private Component testComponent;

		private int intAttribute = 1;
		private string stringAttribute = "test";
		private float floatAttribute = 1.0f;
		private bool boolAttribute = true;


		[SetUp()]
		public void init()
		{
			testEntity = new Entity ();
			testComponent = new Component ();
		}

		#region Test for Setter functions of Attribute (pass if no exception)
		[Test()]
		public void ShouldSetIntAttribute()
		{
			testComponent.setIntAttribute ("attribute", intAttribute);
		}

		[Test()]
		public void ShouldSetFloatAttribute()
		{
			testComponent.setFloatAttribute ("attribute", floatAttribute);
		}

		[Test()]
		public void ShouldSetStringAttribute()
		{
			testComponent.setStringAttribute ("attribute", stringAttribute);
		}

		[Test()]
		public void ShouldSetBoolAttribute()
		{
			testComponent.setBoolAttribute ("attribute", boolAttribute);
		}
		#endregion

		#region Tests for Setter function (pass if return value equals variable used to set attribute)
		[Test()]
		public void ShouldReturnIntAttribute()
		{
			testComponent.setIntAttribute ("attribute", intAttribute);
			int returnValue = testComponent.getIntAttribute ("attribute");
			Assert.IsTrue (returnValue == intAttribute);
		}

		[Test()]
		public void ShouldReturnFloatAttribute()
		{
			testComponent.setFloatAttribute ("attribute", floatAttribute);
			float returnValue = testComponent.getFloatAttribute ("attribute");
			Assert.IsTrue (returnValue == floatAttribute);
		}

		[Test()]
		public void ShouldReturnStringAttribute()
		{
			testComponent.setStringAttribute ("attribute", stringAttribute);
			string returnValue = testComponent.getStringAttribute ("attribute");
			Assert.IsTrue (returnValue == stringAttribute);
		}

		[Test()]
		public void ShouldReturnBoolAttribute()
		{
			testComponent.setBoolAttribute ("attribute", boolAttribute);
			bool returnValue = testComponent.getBoolAttribute ("attribute");
			Assert.IsTrue (returnValue == boolAttribute);
		}
		#endregion

		#region Exception Test (Pass if Getter function for wrong type throws exception)
		[Test()]
		public void ShouldThrowExceptionOnWrongTypeForInt()
		{
			testComponent.setFloatAttribute ("attribute", floatAttribute);
			try
			{
				int returnValue = testComponent.getIntAttribute("attribute");
			}
			catch(AttributeTypeMismatchException e)
			{
				Assert.Pass ("Exception caught: " + e.Message);
			}
			Assert.Fail ("Exception for requesting Float Attribute as INT should have been thrown");
		}

		[Test()]
		public void ShouldThrowExceptionOnWrongTypeForFloat()
		{
			testComponent.setIntAttribute ("attribute", intAttribute);
			try
			{
				float returnValue = testComponent.getFloatAttribute("attribute");
			}
			catch(AttributeTypeMismatchException e)
			{
				Assert.Pass ("Exception caught: " + e.Message);
			}
			Assert.Fail ("Exception for requesting Int Attribute as FLOAT should have been thrown");
		}

		[Test()]
		public void ShouldThrowExceptionOnWrongTypeForString()
		{
			testComponent.setIntAttribute ("attribute", intAttribute);
			try
			{
				string returnValue = testComponent.getStringAttribute("attribute");
			}
			catch(AttributeTypeMismatchException e)
			{
				Assert.Pass ("Exception caught: " + e.Message);
			}
			Assert.Fail ("Exception for requesting Int Attribute as STRING should have been thrown");
		}

		[Test()]
		public void ShouldThrowExceptionOnWrongTypeForBool()
		{
			testComponent.setIntAttribute ("attribute", intAttribute);
			try
			{
				bool returnValue = testComponent.getBoolAttribute("attribute");
			}
			catch(AttributeTypeMismatchException e)
			{
				Assert.Pass ("Exception caught: " + e.Message);
			}
			Assert.Fail ("Exception for requesting Int Attribute as BOOL should have been thrown");
		}
		#endregion
	}
}

