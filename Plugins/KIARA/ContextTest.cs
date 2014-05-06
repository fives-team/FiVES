// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
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

        Mock<IConnectionFactory> mockConnectionFactory;
        Mock<IProtocolRegistry> mockProtocolRegistry;
        Mock<IWebClient> mockWebClient;
        Action<Connection> callback;
        Context context;

        [SetUp()]
        public void Init()
        {
            mockConnectionFactory = new Mock<IConnectionFactory>();
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
                .Setup(registry => registry.GetConnectionFactory("test-protocol-2"))
                .Returns(mockConnectionFactory.Object);

            context = new Context();
            context.protocolRegistry = mockProtocolRegistry.Object;
            context.webClient = mockWebClient.Object;
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
            mockProtocolRegistry.Verify(registry => registry.GetConnectionFactory("test-protocol-2"), Times.Once());
        }

        [Test()]
        public void ShouldChooseServerAccordingToTheFragment()
        {
            context.OpenConnection(configURL + "#1", callback);
            mockProtocolRegistry.Verify(registry => registry.IsRegistered("test-protocol"), Times.Never());
            mockProtocolRegistry.Verify(registry => registry.IsRegistered("test-protocol-2"), Times.Once());
            mockProtocolRegistry.Verify(registry => registry.GetConnectionFactory("test-protocol-2"), Times.Once());
        }

        [Test()]
        public void ShouldOpenConnectionToTheServer()
        {
            context.OpenConnection(configURL, callback);
            mockConnectionFactory.Verify(factory => factory.OpenConnection(It.IsAny<Server>(), context,
                It.IsAny<Action<Connection>>()), Times.Once());
        }

        [Test()]
        public void ShouldStartServer()
        {
            context.StartServer(configURL, callback);
            mockConnectionFactory.Verify(factory => factory.StartServer(It.IsAny<Server>(), context,
                It.IsAny<Action<Connection>>()), Times.Once());
        }
    }
}

