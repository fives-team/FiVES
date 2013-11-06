using NUnit.Framework;
using System;

namespace ClientManagerPlugin
{
    [TestFixture()]
    public class ClientManagerPluginTest
    {
        ClientManagerPluginInitializer plugin = new ClientManagerPluginInitializer();

        [Test()]
        public void ShouldReturnCorrectName()
        {
            Assert.AreEqual("ClientManager", plugin.GetName());
        }

        [Test()]
        public void ShouldReturnCorrectDeps()
        {
            Assert.AreEqual(plugin.GetDependencies().Count, 1);
            Assert.Contains("Auth", plugin.GetDependencies());
        }

        [Test()]
        public void ShouldNotCallOnNewObjectAfterClientDisconnected()
        {
            // TODO
        }

        [Test()]
        public void ShouldNotCallOnRemovedObjectAfterClientDisconnected()
        {
            // TODO
        }
    }
}

