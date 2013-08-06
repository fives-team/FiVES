
using NUnit.Framework;
using System;
using KIARA;

namespace DirectCall
{
    [TestFixture()]
    public class DirectCallPluginTest
    {
        DirectCallPlugin plugin = new DirectCallPlugin();

        public DirectCallPluginTest()
        {
            plugin.initialize();
        }

        [Test()]
        public void shouldRegisterDirectCallProtocol()
        {
            Assert.True(ProtocolRegistry.Instance.isRegistered("direct-call"));
        }

        [Test()]
        public void shouldReturnCorrectName()
        {
            Assert.AreEqual("DirectCall", plugin.getName());
        }

        [Test()]
        public void shouldReturnCorrectDeps()
        {
            Assert.IsEmpty(plugin.getDependencies());
        }
    }
}

