using NUnit.Framework;
using System;

namespace FIVES
{
    [TestFixture()]
    public class ComponentTest
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

            // normally components must be created with ComponentRegistry, but since we are doing unit-test here, it's
            // better to remove dependency on yet another class
            testComponent = new Component ("testComponent");
            testComponent.addAttribute("i", AttributeType.INT);
            testComponent.addAttribute("f", AttributeType.FLOAT);
            testComponent.addAttribute("b", AttributeType.BOOL);
            testComponent.addAttribute("s", AttributeType.STRING);
        }

        [Test()]
        public void shouldAddAttributes()
        {
            // Attributes are already added in init()... just need to check they exist and NULL
            Assert.Null(testComponent.getIntAttribute("i"));
            Assert.Null(testComponent.getFloatAttribute("f"));
            Assert.Null(testComponent.getBoolAttribute("b"));
            Assert.Null(testComponent.getStringAttribute("s"));
        }

        #region Test for Setter functions of Attribute (pass if no exception)
        [Test()]
        public void shouldSetIntAttribute()
        {
            testComponent.setIntAttribute("i", intAttribute);
        }

        [Test()]
        public void shouldSetFloatAttribute()
        {
            testComponent.setFloatAttribute ("f", floatAttribute);
        }

        [Test()]
        public void shouldSetStringAttribute()
        {
            testComponent.setStringAttribute ("s", stringAttribute);
        }

        [Test()]
        public void shouldSetBoolAttribute()
        {
            testComponent.setBoolAttribute ("b", boolAttribute);
        }
        #endregion

        #region Tests for Setter function (pass if return value equals variable used to set attribute)
        [Test()]
        public void shouldReturnIntAttribute()
        {
            testComponent.setIntAttribute ("i", intAttribute);
            int? returnValue = testComponent.getIntAttribute ("i");
            Assert.AreEqual(returnValue, intAttribute);
        }

        [Test()]
        public void shouldReturnFloatAttribute()
        {
            testComponent.setFloatAttribute ("f", floatAttribute);
            float? returnValue = testComponent.getFloatAttribute ("f");
            Assert.AreEqual(returnValue, floatAttribute);
        }

        [Test()]
        public void shouldReturnStringAttribute()
        {
            testComponent.setStringAttribute ("s", stringAttribute);
            string returnValue = testComponent.getStringAttribute ("s");
            Assert.AreEqual(returnValue, stringAttribute);
        }

        [Test()]
        public void shouldReturnBoolAttribute()
        {
            testComponent.setBoolAttribute ("b", boolAttribute);
            bool? returnValue = testComponent.getBoolAttribute ("b");
            Assert.AreEqual(returnValue, boolAttribute);
        }
        #endregion

        #region Exception Test (Pass if Getter function for wrong type throws exception)
        [Test()]
        [ExpectedException(typeof(AttributeTypeMismatchException))]
        public void shouldThrowExceptionOnWrongTypeForInt()
        {
            testComponent.getIntAttribute("f");
        }

        [Test()]
        [ExpectedException(typeof(AttributeTypeMismatchException))]
        public void shouldThrowExceptionOnWrongTypeForFloat()
        {
            testComponent.getFloatAttribute("i");
        }

        [Test()]
        [ExpectedException(typeof(AttributeTypeMismatchException))]
        public void shouldThrowExceptionOnWrongTypeForString()
        {
            testComponent.getStringAttribute("b");
        }

        [Test()]
        [ExpectedException(typeof(AttributeTypeMismatchException))]
        public void shouldThrowExceptionOnWrongTypeForBool()
        {
            testComponent.getBoolAttribute("s");
        }

        [Test()]
        [ExpectedException(typeof(AttributeIsNotDefinedException))]
        public void shouldThrowExceptionWhenSettingUndefinedAttribute() {
            testComponent.setIntAttribute("foobar", 42);
        }

        [Test()]
        [ExpectedException(typeof(AttributeIsNotDefinedException))]
        public void shouldThrowExceptionWhenGettingUndefinedAttribute() {
            testComponent.getIntAttribute("foobar");
        }
        #endregion
    }
}

