using NUnit.Framework;
using System;

namespace ClientManager
{
    [TestFixture()]
    public class ClientManagerPluginTest
    {
        ClientManagerPlugin plugin = new ClientManagerPlugin();

        [Test()]
        public void ShouldReturnCorrectName()
        {
            Assert.AreEqual("ClientManager", plugin.GetName());
        }

        [Test()]
        public void ShouldReturnCorrectDeps()
        {
            Assert.AreEqual(plugin.GetDependencies().Count, 3);
            Assert.Contains("Location", plugin.GetDependencies());
            Assert.Contains("Renderable", plugin.GetDependencies());
            Assert.Contains("Auth", plugin.GetDependencies());
        }
    }
}

