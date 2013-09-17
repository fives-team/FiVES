
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
            plugin.Initialize();
        }

        [Test()]
        public void ShouldRegisterDirectCallProtocol()
        {
            Assert.True(ProtocolRegistry.Instance.IsRegistered("direct-call"));
        }

        [Test()]
        public void ShouldReturnCorrectName()
        {
            Assert.AreEqual("DirectCall", plugin.GetName());
        }

        [Test()]
        public void ShouldReturnCorrectDeps()
        {
            Assert.IsEmpty(plugin.GetDependencies());
        }
    }
}

