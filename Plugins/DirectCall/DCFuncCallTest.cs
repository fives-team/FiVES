
using NUnit.Framework;
using System;

namespace DirectCall
{
    [TestFixture()]
    public class DCFuncCallTest
    {
        private class DCFuncCallWrapper : DCFuncCall
        {
            public object testConvertResult(object result, Type type)
            {
                return base.convertResult(result, type);
            }
        }

        [Test()]
        public void shouldNotConvertResults()
        {
            int iValue = 42;
            float fValue = 3.14f;
            string sValue = "test";
            DCFuncCallWrapper call = new DCFuncCallWrapper();
            Assert.AreEqual(call.testConvertResult(iValue, typeof(string)), iValue);
            Assert.AreEqual(call.testConvertResult(fValue, typeof(int)), fValue);
            Assert.AreEqual(call.testConvertResult(sValue, typeof(float)), sValue);
        }
    }
}

