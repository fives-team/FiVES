using NUnit.Framework;
using System;
using KIARA;

namespace WebSocketJSON
{
    [TestFixture()]
    public class WebSocketJSONPluginTest
    {
        WebSocketJSONPlugin plugin = new WebSocketJSONPlugin();

        public WebSocketJSONPluginTest()
        {
            plugin.Initialize();
        }

        [Test()]
        public void shouldRegisterDirectCallProtocol()
        {
            Assert.True(ProtocolRegistry.Instance.isRegistered("websocket-json"));
        }

        [Test()]
        public void shouldReturnCorrectName()
        {
            Assert.AreEqual("WebSocketJSON", plugin.GetName());
        }

        [Test()]
        public void shouldReturnCorrectDeps()
        {
            Assert.IsEmpty(plugin.GetDependencies());
        }
    }
}

