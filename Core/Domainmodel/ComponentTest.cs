using NUnit.Framework;
using System;
using Microsoft.CSharp.RuntimeBinder;

namespace FIVES
{
    [TestFixture()]
    public class ComponentTest
    {
        private dynamic testComponent;

        private int intAttribute = 1;
        private string stringAttribute = "test";
        private float floatAttribute = 1.0f;
        private bool boolAttribute = true;


        [SetUp()]
        public void init()
        {
            // normally components must be created with ComponentRegistry, but since we are doing unit-test here, it's
            // better to remove dependency on yet another class
            testComponent = new Component ("testComponent");
            testComponent.addAttribute("i", typeof(int), null);
            testComponent.addAttribute("f", typeof(float), null);
            testComponent.addAttribute("b", typeof(bool), null);
            testComponent.addAttribute("s", typeof(string), null);
        }

        [Test()]
        public void shouldAddAttributes()
        {
            // Attributes are already added in init()... just need to check they exist and NULL
            Assert.Null(testComponent.i);
            Assert.Null(testComponent.f);
            Assert.Null(testComponent.b);
            Assert.Null(testComponent.s);
        }

        #region Test for Setter functions of Attribute (pass if no exception)
        [Test()]
        public void shouldSetIntAttribute()
        {
            testComponent.i = intAttribute;
        }

        [Test()]
        public void shouldSetFloatAttribute()
        {
            testComponent.f = floatAttribute;
        }

        [Test()]
        public void shouldSetStringAttribute()
        {
            testComponent.s = stringAttribute;
        }

        [Test()]
        public void shouldSetBoolAttribute()
        {
            testComponent.b = boolAttribute;
        }
        #endregion

        #region Tests for Setter function (pass if return value equals variable used to set attribute)
        [Test()]
        public void shouldReturnIntAttribute()
        {
            testComponent.i = intAttribute;
            int? returnValue = testComponent.i;
            Assert.AreEqual(returnValue, intAttribute);
        }

        [Test()]
        public void shouldReturnFloatAttribute()
        {
            testComponent.f = floatAttribute;
            float? returnValue = testComponent.f;
            Assert.AreEqual(returnValue, floatAttribute);
        }

        [Test()]
        public void shouldReturnStringAttribute()
        {
            testComponent.s = stringAttribute;
            string returnValue = testComponent.s;
            Assert.AreEqual(returnValue, stringAttribute);
        }

        [Test()]
        public void shouldReturnBoolAttribute()
        {
            testComponent.b = boolAttribute;
            bool? returnValue = testComponent.b;
            Assert.AreEqual(returnValue, boolAttribute);
        }
        #endregion

        #region Exception Test (Pass if Getter function for wrong type throws exception)
        [Test()]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void shouldThrowExceptionOnWrongTypeForInt()
        {
            testComponent.f = 1.0f;
            int? result = testComponent.f;
        }

/*        [Test()]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void shouldThrowExceptionOnWrongTypeForFloat()
        {
            testComponent.i = 42;
            float result = testComponent.i;
        }
*/
        [Test()]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void shouldThrowExceptionOnWrongTypeForString()
        {
            testComponent.b = false;
            string result = testComponent.b;
        }

        [Test()]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void shouldThrowExceptionOnWrongTypeForBool()
        {
           testComponent.s = "foobar";
           bool result =  testComponent.s;
        }

        [Test()]
        [ExpectedException(typeof(AttributeIsNotDefinedException))]
        public void shouldThrowExceptionWhenSettingUndefinedAttribute() {
            testComponent.foobar = 42;
        }

        [Test()]
        [ExpectedException(typeof(AttributeIsNotDefinedException))]
        public void shouldThrowExceptionWhenGettingUndefinedAttribute() {
            int? getResult = testComponent.foobar;
        }
        #endregion
    }
}

