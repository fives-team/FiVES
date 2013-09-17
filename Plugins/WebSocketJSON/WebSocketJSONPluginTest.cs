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
        public void ShouldRegisterDirectCallProtocol()
        {
            Assert.True(ProtocolRegistry.Instance.IsRegistered("websocket-json"));
        }

        [Test()]
        public void ShouldReturnCorrectName()
        {
            Assert.AreEqual("WebSocketJSON", plugin.GetName());
        }

        [Test()]
        public void ShouldReturnCorrectDeps()
        {
            Assert.IsEmpty(plugin.GetDependencies());
        }
    }
}

