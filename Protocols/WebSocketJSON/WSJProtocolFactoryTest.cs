using NUnit.Framework;
using System;
using Moq;
using KIARA;
using Newtonsoft.Json;

namespace WebSocketJSON
{
    [TestFixture()]
    public class WSJProtocolFactoryTest
    {
        private Server config = JsonConvert.DeserializeObject<Server>(
            "{ services: '*', protocol: { name: 'websocket-json' } }");
        private Server configWithIpAndPort = JsonConvert.DeserializeObject<Server>(
            "{ services: '*', protocol: { name: 'websocket-json', port: 1234, ip: '127.0.0.1' } }");
        private Server nonWebSocketJSONConfig = JsonConvert.DeserializeObject<Server>(
            "{ services: '*', protocol: { name: 'other-protocol', id: 'test' } }");

        public interface IHandlers {
            void NewClient(IProtocol protocol);
        }

        private WSJProtocolFactory factory;
        private Mock<IWSJServerFactory> mockWSJServerFactory;
        private Mock<IWSJServer> mockWSJServer;
        private Mock<IHandlers> mockHandlers;

        [SetUp()]
        public void Init()
        {
            mockWSJServerFactory = new Mock<IWSJServerFactory>();
            mockWSJServer = new Mock<IWSJServer>();
            mockWSJServerFactory.Setup(f => f.Construct(It.IsAny<Action<IProtocol>>())).Returns(mockWSJServer.Object);
            mockHandlers = new Mock<IHandlers>();

            factory = new WSJProtocolFactory(mockWSJServerFactory.Object);
        }

        [Test()]
        public void ShouldCreateWJSServer()
        {
            factory.StartServer(config, null, mockHandlers.Object.NewClient);
            mockWSJServerFactory.Verify(f => f.Construct(mockHandlers.Object.NewClient), Times.Once());
        }

        [Test()]
        public void ShouldSetupWithPortAndIpFromConfig()
        {
            factory.StartServer(configWithIpAndPort, null, mockHandlers.Object.NewClient);
            mockWSJServer.Verify(s => s.Setup("127.0.0.1", 1234, null, null, null, null, null), Times.Once());
        }

        [Test()]
        public void ShouldSetupWithDefaultPortAndIpIfNotAvailableInConfig()
        {
            factory.StartServer(config, null, mockHandlers.Object.NewClient);
            mockWSJServer.Verify(s => s.Setup("Any", 34837, null, null, null, null, null), Times.Once());
        }

        [Test()]
        public void ShouldStartWJSServer()
        {
            factory.StartServer(config, null, mockHandlers.Object.NewClient);
            mockWSJServer.Verify(s => s.Start(), Times.Once());
        }

        [Test()]
        public void ShouldFailOnConfigForDifferentProtocol()
        {
            Assert.Throws<Error>(
                () => factory.StartServer(nonWebSocketJSONConfig, null, mockHandlers.Object.NewClient));
        }
    }
}

