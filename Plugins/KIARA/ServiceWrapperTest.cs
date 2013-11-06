using NUnit.Framework;
using System;
using Moq;

namespace KIARAPlugin
{
    [TestFixture()]
    public class ServiceWrapperTest
    {
        ServiceWrapper service;
        Context context;
        Connection connection;
        Mock<IProtocol> protocolMock;

        [SetUp()]
        public void Init()
        {
            context = new Context();
            service = new ServiceWrapper(context);
            protocolMock = new Mock<IProtocol>();
            connection = new Connection(protocolMock.Object);
        }

        [Test()]
        public void ShouldInvokeOnConnected()
        {
            bool connected = false;
            service.OnConnected += (c) => connected = true;
            service.HandleConnected(connection);
            Assert.IsTrue(connected);
        }
    }
}

