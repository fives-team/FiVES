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
        public void Init()
        {
            protocolRegistry = new ProtocolRegistry();
        }

        [Test()]
        public void ShouldRegisterNewProtocols()
        {
            var protocolFactory = new Mock<IProtocolFactory>();
            protocolRegistry.RegisterProtocolFactory("test", protocolFactory.Object);
            Assert.True(protocolRegistry.IsRegistered("test"));
            Assert.AreEqual(protocolRegistry.GetProtocolFactory("test"), protocolFactory.Object);
        }

        public void ShouldThrowExceptionWhenAskingForNonRegisteredProtocol()
        {
            Assert.False(protocolRegistry.IsRegistered("unregistered-protocol"));
            Assert.Throws<Error>(() => protocolRegistry.GetProtocolFactory("unregistered-protocol"));
        }
    }
}

