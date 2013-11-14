using NUnit.Framework;
using System;
using Moq;
using KIARAPlugin;
using Newtonsoft.Json;

namespace WebSocketJSON
{
    [TestFixture()]
    public class WSJConnectionFactoryTest
    {
        private Server config = JsonConvert.DeserializeObject<Server>(
            "{ services: '*', protocol: { name: 'websocket-json' } }");
        private Server configWithIpAndPort = JsonConvert.DeserializeObject<Server>(
            "{ services: '*', protocol: { name: 'websocket-json', port: 1234, ip: '127.0.0.1' } }");
        private Server nonWebSocketJSONConfig = JsonConvert.DeserializeObject<Server>(
            "{ services: '*', protocol: { name: 'other-protocol', id: 'test' } }");

        public interface IHandlers {
            void NewClient(Connection connection);
            void OnConnected(Connection connection);
        }

        private WSJConnectionFactory factory;
        private Mock<IWSJServerFactory> mockWSJServerFactory;
        private Mock<IWSJServer> mockWSJServer;
        private Mock<IHandlers> mockHandlers;
        private Mock<IWebSocketFactory> mockWebSocketFactory;
        Mock<IWebSocket> mockWebSocket;

        [SetUp()]
        public void Init()
        {
            mockWSJServerFactory = new Mock<IWSJServerFactory>();
            mockWSJServer = new Mock<IWSJServer>();
            mockWSJServerFactory.Setup(f => f.Construct(It.IsAny<Action<Connection>>())).Returns(mockWSJServer.Object);
            mockHandlers = new Mock<IHandlers>();
            mockWebSocketFactory = new Mock<IWebSocketFactory>();
            mockWebSocket = new Mock<IWebSocket>();

            factory = new WSJConnectionFactory();
            factory.wsjServerFactory = mockWSJServerFactory.Object;
            factory.webSocketFactory = mockWebSocketFactory.Object;
        }

        [Test()]
        public void ShouldCreateWJSServer()
        {
            factory.StartServer(config, null, mockHandlers.Object.NewClient);
            mockWSJServerFactory.Verify(f => f.Construct(mockHandlers.Object.NewClient), Times.Once());
        }

        [Test()]
        public void ShouldUsePortAndIpFromConfig()
        {
            factory.StartServer(configWithIpAndPort, null, mockHandlers.Object.NewClient);
            mockWSJServer.Verify(s => s.Setup("127.0.0.1", 1234, null, null, null, null, null), Times.Once());

            mockWebSocketFactory.Setup(f => f.Construct("ws://127.0.0.1:1234/")).Returns(mockWebSocket.Object);
            factory.OpenConnection(configWithIpAndPort, null, mockHandlers.Object.OnConnected);
            mockWebSocketFactory.Verify(f => f.Construct("ws://127.0.0.1:1234/"));
        }

        [Test()]
        public void ShouldUseDefaultPortAndIpForServerIfNotInConfig()
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
            Assert.Throws<Error>(() => factory.StartServer(nonWebSocketJSONConfig, null,
                mockHandlers.Object.NewClient));
            Assert.Throws<Error>(() => factory.OpenConnection(nonWebSocketJSONConfig, null,
                mockHandlers.Object.OnConnected));
        }

        [Test()]
        public void ShouldOpenWebSocketConnection()
        {
            mockWebSocketFactory.Setup(f => f.Construct("ws://127.0.0.1:1234/")).Returns(mockWebSocket.Object);
            factory.OpenConnection(configWithIpAndPort, null, mockHandlers.Object.OnConnected);
            mockWebSocket.Verify(s => s.Open());
        }

        [Test()]
        public void ShouldInvokeOnConnectedHandler()
        {
            mockWebSocketFactory.Setup(f => f.Construct("ws://127.0.0.1:1234/")).Returns(mockWebSocket.Object);
            factory.OpenConnection(configWithIpAndPort, null, mockHandlers.Object.OnConnected);
            mockWebSocket.Raise(s => s.Opened += null, new EventArgs());
            mockHandlers.Verify(h => h.OnConnected(It.IsAny<Connection>()), Times.Once());
        }

        [Test()]
        [ExpectedException(typeof(Error))]
        public void ShouldFailIfPortOrIpAreNotInConfigForClient()
        {
            factory.OpenConnection(config, null, mockHandlers.Object.OnConnected);
        }
    }
}

