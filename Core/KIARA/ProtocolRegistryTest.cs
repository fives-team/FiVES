using System;
using NUnit.Framework;
using Moq;

namespace KIARA
{
    [TestFixture()]
    public class ProtocolRegistryTest
    {
        private ProtocolRegistry protocolRegistry;

        [SetUp()]
        public void init()
        {
            protocolRegistry = new ProtocolRegistry();
        }

        [Test()]
        public void shouldRegisterNewProtocols()
        {
            var protocolFactory = new Mock<IProtocolFactory>();
            protocolRegistry.registerProtocolFactory("test", protocolFactory.Object);
            Assert.True(protocolRegistry.isRegistered("test"));
            Assert.AreEqual(protocolRegistry.getProtocolFactory("test"), protocolFactory.Object);
        }

        public void shouldThrowExceptionWhenAskingForNonRegisteredProtocol()
        {
            Assert.False(protocolRegistry.isRegistered("unregistered-protocol"));
            Assert.Throws<Error>(() => protocolRegistry.getProtocolFactory("unregistered-protocol"));
        }
    }
}

