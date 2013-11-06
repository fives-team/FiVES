using System;
using NUnit.Framework;
using Moq;

namespace KIARAPlugin
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
        public void Init()
        {
            mockProtocolFactory = new Mock<IProtocolFactory>();
            mockProtocolRegistry = new Mock<IProtocolRegistry>();
            mockWebClient = new Mock<IWebClient>();
            callback = delegate(Connection obj) {};

            mockWebClient
                .Setup(client => client.DownloadString(configURL))
                .Returns(testConfig);
            mockProtocolRegistry
                .Setup(registry => registry.IsRegistered("test-protocol"))
                .Returns(false);
            mockProtocolRegistry
                .Setup(registry => registry.IsRegistered("test-protocol-2"))
                .Returns(true);
            mockProtocolRegistry
                .Setup(registry => registry.GetProtocolFactory("test-protocol-2"))
                .Returns(mockProtocolFactory.Object);

            context = new Context(mockProtocolRegistry.Object, mockWebClient.Object);
        }

        [Test()]
        public void ShouldRetrieveConfigOnConnect()
        {
            context.OpenConnection(configURL, callback);
            mockWebClient.Verify(client => client.DownloadString(configURL), Times.Once());
        }

        [Test()]
        public void ShouldRetrieveConfigOnStartServer()
        {
            context.StartServer(configURL, callback);
            mockWebClient.Verify(client => client.DownloadString(configURL), Times.Once());
        }

        [Test()]
        public void ShouldSearchForSupportedServer()
        {
            context.OpenConnection(configURL, callback);
            mockProtocolRegistry.Verify(registry => registry.IsRegistered("test-protocol"), Times.Once());
            mockProtocolRegistry.Verify(registry => registry.IsRegistered("test-protocol-2"), Times.Once());
            mockProtocolRegistry.Verify(registry => registry.GetProtocolFactory("test-protocol-2"), Times.Once());
        }

        [Test()]
        public void ShouldChooseServerAccordingToTheFragment()
        {
            context.OpenConnection(configURL + "#1", callback);
            mockProtocolRegistry.Verify(registry => registry.IsRegistered("test-protocol"), Times.Never());
            mockProtocolRegistry.Verify(registry => registry.IsRegistered("test-protocol-2"), Times.Once());
            mockProtocolRegistry.Verify(registry => registry.GetProtocolFactory("test-protocol-2"), Times.Once());
        }

        [Test()]
        public void ShouldOpenConnectionToTheServer()
        {
            context.OpenConnection(configURL, callback);
            mockProtocolFactory.Verify(factory => factory.OpenConnection(It.IsAny<Server>(),
                                                                         context,
                                                                         It.IsAny<Action<IProtocol>>()), Times.Once());
        }

        [Test()]
        public void ShouldStartServer()
        {
            context.StartServer(configURL, callback);
            mockProtocolFactory.Verify(factory => factory.StartServer(It.IsAny<Server>(), 
                                                                      context,
                                                                      It.IsAny<Action<IProtocol>>()), Times.Once());
        }

        [Test()]
        public void ShouldStoreAndReturnProtocolData()
        {
            context.ProtocolData["test-protocol-2"] = 42;
            Assert.AreEqual(42, context.ProtocolData["test-protocol-2"]);
        }

        [Test()]
        public void ShouldUseHintAsConfigTemplate()
        {
            context.Initialize("test {0} template");
            Assert.AreEqual("test {0} template", context.configTemplate);
        }
    }
}

