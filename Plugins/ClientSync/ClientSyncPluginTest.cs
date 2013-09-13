
using NUnit.Framework;
using System;

namespace ClientSync
{
    [TestFixture()]
    public class ClientSyncPluginTest
    {
        ClientSyncPlugin plugin = new ClientSyncPlugin();

        [Test()]
        public void ShouldReturnCorrectName()
        {
            Assert.AreEqual("ClientSync", plugin.GetName());
        }

        [Test()]
        public void ShouldReturnCorrectDeps()
        {
            Assert.AreEqual(plugin.GetDependencies().Count, 5);
            Assert.Contains("WebSocketJSON", plugin.GetDependencies());
            Assert.Contains("DirectCall", plugin.GetDependencies());
            Assert.Contains("Location", plugin.GetDependencies());
            Assert.Contains("Renderable", plugin.GetDependencies());
            Assert.Contains("Auth", plugin.GetDependencies());
        }
    }
}

