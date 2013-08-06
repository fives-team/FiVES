using System;
using NUnit.Framework;
using Moq;

namespace KIARA
{
    [TestFixture()]
    public class ConnectionTest
    {
        private string idlURL = "http://www.example.com/my.idl";

        [Test()]
        public void shouldCallFuncInProtocol()
        {
            var mockProtocol = new Mock<IProtocol>();

            Connection c = new Connection(mockProtocol.Object);
            var testFunc = c.generateFuncWrapper("testFunc");
            testFunc("testArg", 42);

            mockProtocol.Verify(protocol => protocol.callFunc("testFunc", "testArg", 42), Times.Once());
        }

        [Test()]
        public void shouldDownloadIDLAndCallProcessIDLInProtocol()
        {
            var mockProtocol = new Mock<IProtocol>();
            var mockWebClient = new Mock<IWebClient>();

            mockWebClient.Setup(client => client.DownloadString(idlURL)).Returns("{}");

            Connection c = new Connection(mockProtocol.Object, mockWebClient.Object);
            c.loadIDL(idlURL);

            mockWebClient.Verify(client => client.DownloadString(idlURL), Times.Once());
            mockProtocol.Verify(protocol => protocol.processIDL("{}"), Times.Once());
        }

        [Test()]
        public void shouldCallRegisterHandlerInProtocol()
        {
            var mockProtocol = new Mock<IProtocol>();

            Connection c = new Connection(mockProtocol.Object);
            Delegate d = (Func<int, string>)delegate(int x) { return ""; };
            c.registerFuncImplementation("testFunc", d);

            mockProtocol.Verify(protocol => protocol.registerHandler("testFunc", d), Times.Once());
        }

        // TODO: Tests for type mapping
    }
}

