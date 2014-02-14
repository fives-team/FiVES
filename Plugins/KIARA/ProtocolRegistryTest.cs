using System;
using NUnit.Framework;
using Moq;

namespace KIARAPlugin
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
            var connectionFactory = new Mock<IConnectionFactory>();
            protocolRegistry.RegisterConnectionFactory("test", connectionFactory.Object);
            Assert.True(protocolRegistry.IsRegistered("test"));
            Assert.AreEqual(protocolRegistry.GetConnectionFactory("test"), connectionFactory.Object);
        }

        public void ShouldThrowExceptionWhenAskingForNonRegisteredProtocol()
        {
            Assert.False(protocolRegistry.IsRegistered("unregistered-protocol"));
            Assert.Throws<Error>(() => protocolRegistry.GetConnectionFactory("unregistered-protocol"));
        }
    }
}

