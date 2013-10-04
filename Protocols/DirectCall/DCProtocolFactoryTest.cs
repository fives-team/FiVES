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
            void NewClient(IProtocol protocol);
            void Connected(IProtocol protocol);
        }

        private DCProtocolFactory factory;
        private Mock<IHandlers> mockHandlers;
        private Context context;

        [SetUp()]
        public void Init()
        {
            factory = new DCProtocolFactory();
            mockHandlers = new Mock<IHandlers>();
            context = new Context();
        }

        [Test()]
        public void ShouldCallConnectedCallback()
        {
            factory.StartServer(config, context, mockHandlers.Object.NewClient);
            factory.OpenConnection(config, context, mockHandlers.Object.Connected);
            mockHandlers.Verify(h => h.Connected(It.IsAny<IProtocol>()), Times.Once());
        }

        [Test()]
        public void ShouldCallNewClientCallback()
        {
            factory.StartServer(config, context, mockHandlers.Object.NewClient);
            factory.OpenConnection(config, context, mockHandlers.Object.Connected);
            mockHandlers.Verify(h => h.NewClient(It.IsAny<IProtocol>()), Times.Once());
        }

        [Test()]
        public void ShouldFailToRecreateServerWithTheSameId()
        {
            factory.StartServer(config, context, mockHandlers.Object.NewClient);
            Assert.Throws<Error>(() => factory.StartServer(config, context, mockHandlers.Object.NewClient));
        }

        [Test()]
        public void ShouldFailOnConfigForDifferentProtocol()
        {
            Assert.Throws<Error>(
                () => factory.StartServer(nonDirectCallConfig, context, mockHandlers.Object.NewClient));
            Assert.Throws<Error>(
                () => factory.OpenConnection(nonDirectCallConfig, context, mockHandlers.Object.Connected));
        }

        [Test()]
        public void ShouldFailToUseConfigWithoutId()
        {
            Assert.Throws<Error>(() => factory.OpenConnection(configWithoutId, context, mockHandlers.Object.Connected));
            Assert.Throws<Error>(() => factory.StartServer(configWithoutId, context, mockHandlers.Object.NewClient));
        }

        [Test()]
        public void ShouldFailToConnectWhenServerWithGivenIdIsNotStarted()
        {

        }

        [Test()]
        public void ShouldConstructDCProtocol()
        {
            factory.StartServer(config, context, mockHandlers.Object.NewClient);
            factory.OpenConnection(config, context, mockHandlers.Object.Connected);
            mockHandlers.Verify(h => h.Connected(It.IsAny<DCProtocol>()), Times.Once());
            mockHandlers.Verify(h => h.NewClient(It.IsAny<DCProtocol>()), Times.Once());
        }
    }
}

