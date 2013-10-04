
using NUnit.Framework;
using System;

namespace DirectCall
{
    [TestFixture()]
    public class DCFuncCallTest : DCFuncCall
    {
        [Test()]
        public void ShouldNotConvertResults()
        {
            int iValue = 42;
            float fValue = 3.14f;
            string sValue = "test";
            Assert.AreEqual(base.ConvertResult(iValue, typeof(string)), iValue);
            Assert.AreEqual(base.ConvertResult(fValue, typeof(int)), fValue);
            Assert.AreEqual(base.ConvertResult(sValue, typeof(float)), sValue);
        }
    }
}

