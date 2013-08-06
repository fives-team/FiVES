using NUnit.Framework;
using System;
using KIARA;
using Moq;
using Newtonsoft.Json;

namespace DirectCall
{
    [TestFixture()]
    public class DCProtocolFactoryTest
    {
        private Server config = JsonConvert.DeserializeObject<Server>(
            "{ services: '*', protocol: { name: 'direct-call', id: 'test' } }");
        private Server configWithoutId = JsonConvert.DeserializeObject<Server>(
            "{ services: '*', protocol: { name: 'direct-call' } }");
        private Server nonDirectCallConfig = JsonConvert.DeserializeObject<Server>(
            "{ services: '*', protocol: { name: 'other-protocol', id: 'test' } }");

        public interface IHandlers {
            void newClient(IProtocol protocol);
            void connected(IProtocol protocol);
        }

        private DCProtocolFactory factory;
        private Mock<IHandlers> handlers;

        [SetUp()]
        public void init()
        {
            factory = new DCProtocolFactory();
            handlers = new Mock<IHandlers>();
        }

        [Test()]
        public void shouldCallConnectedCallback()
        {
            factory.startServer(config, handlers.Object.newClient);
            factory.openConnection(config, handlers.Object.connected);
            handlers.Verify(h => h.connected(It.IsAny<IProtocol>()), Times.Once());
        }

        [Test()]
        public void shouldCallNewClientCallback()
        {
            factory.startServer(config, handlers.Object.newClient);
            factory.openConnection(config, handlers.Object.connected);
            handlers.Verify(h => h.newClient(It.IsAny<IProtocol>()), Times.Once());
        }

        [Test()]
        public void shouldFailToRecreateServerWithTheSameId()
        {
            factory.startServer(config, handlers.Object.newClient);
            Assert.Throws<Error>(() => factory.startServer(config, handlers.Object.newClient));
        }

        [Test()]
        public void shouldFailOnConfigForDifferentProtocol()
        {
            Assert.Throws<Error>(() => factory.startServer(nonDirectCallConfig, handlers.Object.newClient));
            Assert.Throws<Error>(() => factory.openConnection(nonDirectCallConfig, handlers.Object.connected));
        }

        [Test()]
        public void shouldFailToUseConfigWithoutId()
        {
            Assert.Throws<Error>(() => factory.openConnection(configWithoutId, handlers.Object.connected));
            Assert.Throws<Error>(() => factory.startServer(configWithoutId, handlers.Object.newClient));
        }

        [Test()]
        public void shouldFailToConnectWhenServerWithGivenIdIsNotStarted()
        {

        }

        [Test()]
        public void shouldConstructDCProtocol()
        {
            factory.startServer(config, handlers.Object.newClient);
            factory.openConnection(config, handlers.Object.connected);
            handlers.Verify(h => h.connected(It.IsAny<DCProtocol>()), Times.Once());
            handlers.Verify(h => h.newClient(It.IsAny<DCProtocol>()), Times.Once());
        }
    }
}

