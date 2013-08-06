using System;
using NUnit.Framework;
using NUnit.Mocks;
using Moq;

namespace KIARA
{
    [TestFixture()]
    public class ContextTest
    {
        string configURL = "http://www.example.com/config.json";
        string testConfig = 
            "{ " +
            "  idlContents: '', " +
            "  info: 'TestServer', " + 
            "  servers: [" +
            "    { " +
            "      services: '*'," +
            "      protocol: { name: 'test-protocol' } " +
            "    }, " +
            "    {" +
            "      services: '*'," +
            "      protocol: { name: 'test-protocol-2' }" +
            "    }" +
            "  ] " +
            "}";

        Mock<IProtocolFactory> mockProtocolFactory;
        Mock<IProtocolRegistry> mockProtocolRegistry;
        Mock<IWebClient> mockWebClient;
        Action<Connection> callback;
        Context context;

        [SetUp()]
        public void init()
        {
            mockProtocolFactory = new Mock<IProtocolFactory>();
            mockProtocolRegistry = new Mock<IProtocolRegistry>();
            mockWebClient = new Mock<IWebClient>();
            callback = delegate(Connection obj) {};

            mockWebClient
                .Setup(client => client.DownloadString(configURL))
                .Returns(testConfig);
            mockProtocolRegistry
                .Setup(registry => registry.isRegistered("test-protocol"))
                .Returns(false);
            mockProtocolRegistry
                .Setup(registry => registry.isRegistered("test-protocol-2"))
                .Returns(true);
            mockProtocolRegistry
                .Setup(registry => registry.getProtocolFactory("test-protocol-2"))
                .Returns(mockProtocolFactory.Object);

            context = new Context(mockProtocolRegistry.Object, mockWebClient.Object);
        }

        [Test()]
        public void shouldRetrieveConfigOnConnect()
        {
            context.openConnection(configURL, callback);
            mockWebClient.Verify(client => client.DownloadString(configURL), Times.Once());
        }

        [Test()]
        public void shouldRetrieveConfigOnStartServer()
        {
            context.startServer(configURL, callback);
            mockWebClient.Verify(client => client.DownloadString(configURL), Times.Once());
        }

        [Test()]
        public void shouldSearchForSupportedServer()
        {
            context.openConnection(configURL, callback);
            mockProtocolRegistry.Verify(registry => registry.isRegistered("test-protocol"), Times.Once());
            mockProtocolRegistry.Verify(registry => registry.isRegistered("test-protocol-2"), Times.Once());
            mockProtocolRegistry.Verify(registry => registry.getProtocolFactory("test-protocol-2"), Times.Once());
        }

        [Test()]
        public void shouldChooseServerAccordingToTheFragment()
        {
            context.openConnection(configURL + "#1", callback);
            mockProtocolRegistry.Verify(registry => registry.isRegistered("test-protocol"), Times.Never());
            mockProtocolRegistry.Verify(registry => registry.isRegistered("test-protocol-2"), Times.Once());
            mockProtocolRegistry.Verify(registry => registry.getProtocolFactory("test-protocol-2"), Times.Once());
        }

        [Test()]
        public void shouldOpenConnectionToTheServer()
        {
            context.openConnection(configURL, callback);
            mockProtocolFactory.Verify(factory => factory.openConnection(It.IsAny<Server>(), 
                                                                         It.IsAny<Action<IProtocol>>()), Times.Once());
        }

        [Test()]
        public void shouldStartServer()
        {
            context.startServer(configURL, callback);
            mockProtocolFactory.Verify(factory => factory.startServer(It.IsAny<Server>(), 
                                                                      It.IsAny<Action<IProtocol>>()), Times.Once());
        }
    }
}

