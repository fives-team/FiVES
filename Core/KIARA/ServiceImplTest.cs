using NUnit.Framework;
using System;
using Moq;

namespace KIARA
{
    [TestFixture()]
    public class ServiceImplTest
    {
        ServiceImpl service;
        Context context;
        Connection connection;
        Mock<IProtocol> protocolMock;

        [SetUp()]
        public void init()
        {
            context = new Context();
            service = new ServiceImpl(context);
            protocolMock = new Mock<IProtocol>();
            connection = new Connection(protocolMock.Object);
        }

        [Test()]
        public void shouldInvokeOnNewClient()
        {
            int numConnectedClients =  0;
            service.OnNewClient += (c) => numConnectedClients++;
            service.HandleNewClient(connection);
            Assert.AreEqual(1, numConnectedClients);
        }

        [Test()]
        public void shouldRegisterMethodsForNewConnections()
        {
            Delegate d1 = (Action) delegate {};
            Delegate d2 = (Func<string, int>) delegate(string arg) { return 42; };

            service["foobar"] = d1;
            service["barfoo"] = d2;
            service.HandleNewClient(connection);
            protocolMock.Verify(p => p.registerHandler("foobar", d1));
            protocolMock.Verify(p => p.registerHandler("barfoo", d2));
        }
    }
}

