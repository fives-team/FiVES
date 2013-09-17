using NUnit.Framework;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebSocketJSON
{
    [TestFixture()]
    public class WSJFuncCallTest : WSJFuncCall
    {
        [Test()]
        public void ShouldConvertResults()
        {
            Assert.AreEqual(base.ConvertResult(JsonConvert.DeserializeObject<JToken>("42"), typeof(int)), 42);
            Assert.AreEqual(base.ConvertResult(JsonConvert.DeserializeObject<JToken>("'abc'"), typeof(string)), "abc");
            Assert.AreEqual(base.ConvertResult(JsonConvert.DeserializeObject<JToken>("3.14"), typeof(float)), 3.14f);
        }
    }
}

