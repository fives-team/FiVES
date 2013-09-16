
using NUnit.Framework;
using System;

namespace ClientSync
{
    [TestFixture()]
    public class ClientSyncPluginTest
    {
        ClientSyncPlugin plugin = new ClientSyncPlugin();

        [Test()]
        public void shouldReturnCorrectName()
        {
            Assert.AreEqual("ClientSync", plugin.GetName());
        }

        [Test()]
        public void shouldReturnCorrectDeps()
        {
            Assert.AreEqual(plugin.GetDependencies().Count, 4);
            Assert.Contains("WebSocketJSON", plugin.GetDependencies());
            Assert.Contains("DirectCall", plugin.GetDependencies());
            Assert.Contains("Location", plugin.GetDependencies());
            Assert.Contains("Renderable", plugin.GetDependencies());
        }
    }
}

