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
        Mock<Connection> connectionMock;

        [SetUp()]
        public void Init()
        {
            context = new Context();
            service = new ServiceWrapper(context);
            connectionMock = new Mock<Connection>();
        }

        [Test()]
        public void ShouldInvokeOnConnected()
        {
            bool connected = false;
            service.OnConnected += (c) => connected = true;
            service.HandleConnected(connectionMock.Object);
            Assert.IsTrue(connected);
        }
    }
}

