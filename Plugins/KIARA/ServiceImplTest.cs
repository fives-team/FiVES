using NUnit.Framework;
using System;
using Moq;

namespace KIARAPlugin
{
    [TestFixture()]
    public class ServiceImplTest
    {
        ServiceImpl service;
        Context context;
        Connection connection;
        Mock<IProtocol> protocolMock;

        [SetUp()]
        public void Init()
        {
            context = new Context();
            service = new ServiceImpl(context);
            protocolMock = new Mock<IProtocol>();
            connection = new Connection(protocolMock.Object);
        }

        [Test()]
        public void ShouldInvokeOnNewClient()
        {
            int numConnectedClients =  0;
            service.OnNewClient += (c) => numConnectedClients++;
            service.HandleNewClient(connection);
            Assert.AreEqual(1, numConnectedClients);
        }

        [Test()]
        public void ShouldRegisterMethodsForNewConnections()
        {
            Delegate d1 = (Action) delegate {};
            Delegate d2 = (Func<string, int>) delegate(string arg) { return 42; };

            service["foobar"] = d1;
            service["barfoo"] = d2;
            service.HandleNewClient(connection);
            protocolMock.Verify(p => p.RegisterHandler("foobar", d1));
            protocolMock.Verify(p => p.RegisterHandler("barfoo", d2));
        }
    }
}

