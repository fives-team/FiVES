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
            void newClient(IProtocol protocol);
        }

        private WSJProtocolFactory factory;
        private Mock<IWSJServerFactory> mockWSJServerFactory;
        private Mock<IWSJServer> mockWSJServer;
        private Mock<IHandlers> mockHandlers;

        [SetUp()]
        public void init()
        {
            mockWSJServerFactory = new Mock<IWSJServerFactory>();
            mockWSJServer = new Mock<IWSJServer>();
            mockWSJServerFactory.Setup(f => f.construct(It.IsAny<Action<IProtocol>>())).Returns(mockWSJServer.Object);
            mockHandlers = new Mock<IHandlers>();

            factory = new WSJProtocolFactory(mockWSJServerFactory.Object);
        }

        [Test()]
        public void shouldCreateWJSServer()
        {
            factory.startServer(config, mockHandlers.Object.newClient);
            mockWSJServerFactory.Verify(f => f.construct(mockHandlers.Object.newClient), Times.Once());
        }

        [Test()]
        public void shouldSetupWithPortAndIpFromConfig()
        {
            factory.startServer(configWithIpAndPort, mockHandlers.Object.newClient);
            mockWSJServer.Verify(s => s.Setup("127.0.0.1", 1234, null, null, null, null, null), Times.Once());
        }

        [Test()]
        public void shouldSetupWithDefaultPortAndIpIfNotAvailableInConfig()
        {
            factory.startServer(config, mockHandlers.Object.newClient);
            mockWSJServer.Verify(s => s.Setup("Any", 34837, null, null, null, null, null), Times.Once());
        }

        [Test()]
        public void shouldStartWJSServer()
        {
            factory.startServer(config, mockHandlers.Object.newClient);
            mockWSJServer.Verify(s => s.Start(), Times.Once());
        }

        [Test()]
        public void shouldFailOnConfigForDifferentProtocol()
        {
            Assert.Throws<Error>(() => factory.startServer(nonWebSocketJSONConfig, mockHandlers.Object.newClient));
        }
    }
}

