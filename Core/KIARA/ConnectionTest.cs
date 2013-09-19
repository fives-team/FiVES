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
        public void ShouldCallFuncInProtocol()
        {
            var mockProtocol = new Mock<IProtocol>();

            Connection c = new Connection(mockProtocol.Object);
            var testFunc = c.GenerateFuncWrapper("testFunc");
            testFunc("testArg", 42);

            mockProtocol.Verify(protocol => protocol.CallFunc("testFunc", "testArg", 42), Times.Once());
        }

        [Test()]
        public void ShouldDownloadIDLAndCallProcessIDLInProtocol()
        {
            var mockProtocol = new Mock<IProtocol>();
            var mockWebClient = new Mock<IWebClient>();

            mockWebClient.Setup(client => client.DownloadString(idlURL)).Returns("{}");

            Connection c = new Connection(mockProtocol.Object, mockWebClient.Object);
            c.LoadIDL(idlURL);

            mockWebClient.Verify(client => client.DownloadString(idlURL), Times.Once());
            mockProtocol.Verify(protocol => protocol.ProcessIDL("{}"), Times.Once());
        }

        [Test()]
        public void ShouldCallRegisterHandlerInProtocol()
        {
            var mockProtocol = new Mock<IProtocol>();

            Connection c = new Connection(mockProtocol.Object);
            Delegate d = (Func<int, string>)delegate(int x) { return ""; };
            c.RegisterFuncImplementation("testFunc", d);

            mockProtocol.Verify(protocol => protocol.RegisterHandler("testFunc", d), Times.Once());
        }


        [Test()]
        public void ShouldMapBracketOperatorToGenerateFuncWrapper()
        {
            var mockProtocol = new Mock<IProtocol>();
            Connection c = new Connection(mockProtocol.Object);
            c["foo"](123);
            mockProtocol.Verify(p => p.CallFunc("foo", 123), Times.Once());
        }

        // TODO: Tests for type mapping
    }
}

