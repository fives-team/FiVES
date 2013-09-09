using NUnit.Framework;
using System;
using Moq;

namespace KIARA
{
    [TestFixture()]
    public class ServiceWrapperTest
    {
        ServiceWrapper service;
        Context context;
        Connection connection;
        Mock<IProtocol> protocolMock;

        [SetUp()]
        public void init()
        {
            context = new Context();
            service = new ServiceWrapper(context);
            protocolMock = new Mock<IProtocol>();
            connection = new Connection(protocolMock.Object);
        }

        [Test()]
        public void shouldGenerateCorrectMethodWrappers()
        {
            service.HandleConnected(connection);
            service["foobar"](123);
            protocolMock.Verify(p => p.callFunc("foobar", 123), Times.Once());
        }

        [Test()]
        public void shouldInvokeOnConnected()
        {
            bool connected = false;
            service.OnConnected += (c) => connected = true;
            service.HandleConnected(connection);
            Assert.IsTrue(connected);
        }

        [Test()]
        public void shouldThrowExceptionOnGetterBeforeConnected()
        {
            Assert.Throws<Error>(() => service["foobar"]());
        }
    }
}

