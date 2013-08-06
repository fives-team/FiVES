
using NUnit.Framework;
using System;

namespace ClientSync
{
    [TestFixture()]
    public class ClientSyncPluginTest
    {
        ClientSyncPlugin plugin = new ClientSyncPlugin();

        public ClientSyncPluginTest()
        {
            plugin.initialize();
        }

        [Test()]
        public void shouldReturnCorrectName()
        {
            Assert.AreEqual("ClientSync", plugin.getName());
        }

        [Test()]
        public void shouldReturnCorrectDeps()
        {
            Assert.Contains("WebSocketJSON", plugin.getDependencies());
            Assert.Contains("DirectCall", plugin.getDependencies());
        }
    }
}

