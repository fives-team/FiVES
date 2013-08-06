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
        public void shouldConvertResults()
        {
            Assert.AreEqual(base.convertResult(JsonConvert.DeserializeObject<JToken>("42"), typeof(int)), 42);
            Assert.AreEqual(base.convertResult(JsonConvert.DeserializeObject<JToken>("'abc'"), typeof(string)), "abc");
            Assert.AreEqual(base.convertResult(JsonConvert.DeserializeObject<JToken>("3.14"), typeof(float)), 3.14f);
        }
    }
}

