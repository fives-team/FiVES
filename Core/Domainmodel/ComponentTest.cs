using NUnit.Framework;
using System;
using Microsoft.CSharp.RuntimeBinder;

namespace FIVES
{
    [TestFixture()]
    public class ComponentTest
    {
        private Component testComponent;

        private int intAttribute = 1;
        private string stringAttribute = "test";
        private float floatAttribute = 1.0f;
        private bool boolAttribute = true;


        [SetUp()]
        public void Init()
        {
            // normally components must be created with ComponentRegistry, but since we are doing unit-test here, it's
            // better to remove dependency on yet another class
            testComponent = new Component ("testComponent");            testComponent.AddAttribute("i", typeof(int), null);
            testComponent.AddAttribute("f", typeof(float), null);
            testComponent.AddAttribute("b", typeof(bool), null);
            testComponent.AddAttribute("s", typeof(string), null);
        }

        [Test()]
        public void ShouldAddAttributes()
        {
            // Attributes are already added in Init()... just need to check they exist and NULL
            Assert.Null(testComponent["i"]);
            Assert.Null(testComponent["f"]);
            Assert.Null(testComponent["b"]);
            Assert.Null(testComponent["s"]);
        }

        [Test()]
        public void ShouldResetAttributeToDefaultValue()
        {
            var layout = new ComponentLayout();
            layout.AddAttribute<int>("i", 42);
            ComponentRegistry.Instance.DefineComponent("testComponent", Guid.NewGuid(), layout);

            testComponent["i"] = 33;
            testComponent.ResetAttributeValue("i");
            Assert.AreEqual(42, (int)testComponent["i"]);
        }

        #region Test for Setter functions of Attribute (pass if no exception)
        [Test()]
        public void ShouldSetIntAttribute()
        {
            testComponent["i"] = intAttribute;
        }

        [Test()]
        public void ShouldSetFloatAttribute()
        {
            testComponent["f"] = floatAttribute;
        }

        [Test()]
        public void ShouldSetStringAttribute()
        {
            testComponent["s"] = stringAttribute;
        }

        [Test()]
        public void ShouldSetBoolAttribute()
        {
            testComponent["b"] = boolAttribute;
        }
        #endregion

        #region Tests for Setter function (pass if return value equals variable used to set attribute)
        [Test()]
        public void ShouldReturnIntAttribute()
        {
            testComponent["i"] = intAttribute;
            int? returnValue = (int)testComponent["i"];
            Assert.AreEqual(returnValue, intAttribute);
        }

        [Test()]
        public void ShouldReturnFloatAttribute()
        {
            testComponent["f"] = floatAttribute;
            float? returnValue = (float)testComponent["f"];
            Assert.AreEqual(returnValue, floatAttribute);
        }

        [Test()]
        public void ShouldReturnStringAttribute()
        {
            testComponent["s"] = stringAttribute;
            string returnValue = (string)testComponent["s"];
            Assert.AreEqual(returnValue, stringAttribute);
        }

        [Test()]
        public void ShouldReturnBoolAttribute()
        {
            testComponent["b"] = boolAttribute;
            bool? returnValue = (bool)testComponent["b"];
            Assert.AreEqual(returnValue, boolAttribute);
        }
        #endregion

        #region Exception Test (Pass if Getter function for wrong type throws exception)
        [Test()]
        [ExpectedException(typeof(InvalidCastException))]
        public void ShouldThrowExceptionOnWrongTypeForInt()
        {
            testComponent["f"] = 1.0f;
            int? result = (int)testComponent["f"];
        }

/*        [Test()]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ShouldThrowExceptionOnWrongTypeForFloat()
        {
            testComponent["i"] = 42;
            float result = testComponent["i"];
        }
*/
        [Test()]        [ExpectedException(typeof(InvalidCastException))]
        public void ShouldThrowExceptionOnWrongTypeForString()
        {
            testComponent["b"] = false;
            string result = (string)testComponent["b"];
        }

        [Test()]
        [ExpectedException(typeof(InvalidCastException))]
        public void ShouldThrowExceptionOnWrongTypeForBool()
        {
           testComponent["s"] = "foobar";
           bool result =  (bool)testComponent["s"];
        }

        [Test()]
        [ExpectedException(typeof(AttributeIsNotDefinedException))]
        public void ShouldThrowExceptionWhenSettingUndefinedAttribute() {
            testComponent["foobar"] = 42;
        }

        [Test()]
        [ExpectedException(typeof(AttributeIsNotDefinedException))]
        public void ShouldThrowExceptionWhenGettingUndefinedAttribute() {
            int? getResult = (int)testComponent["foobar"];
        }
        #endregion
    }
}

