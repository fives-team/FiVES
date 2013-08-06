
using NUnit.Framework;
using System;

namespace DirectCall
{
    [TestFixture()]
    public class DCFuncCallTest : DCFuncCall
    {
        [Test()]
        public void shouldNotConvertResults()
        {
            int iValue = 42;
            float fValue = 3.14f;
            string sValue = "test";
            Assert.AreEqual(base.convertResult(iValue, typeof(string)), iValue);
            Assert.AreEqual(base.convertResult(fValue, typeof(int)), fValue);
            Assert.AreEqual(base.convertResult(sValue, typeof(float)), sValue);
        }
    }
}

