using System;
using NUnit.Framework;
using Moq;

namespace KIARAPlugin
{
    [TestFixture()]
    public class ConnectionTest
    {
        private string idlURL = "http://www.example.com/my.idl";

        public class ConnectionImpl : Connection
        {
            public override event EventHandler Closed;

            public override void Disconnect()
            {
            }

            protected override void ProcessIDL(string parsedIDL)
            {
                ProcessIDLTest(parsedIDL);
            }

            protected override IFuncCall CallFunc(string funcName, params object[] args)
            {
                return CallFuncTest(funcName, args);
            }

            protected override void RegisterHandler(string funcName, Delegate handler)
            {
                RegisterHandlerTest(funcName, handler);
            }

            public virtual void ProcessIDLTest(string parsedIDL)
            {
            }

            public virtual IFuncCall CallFuncTest(string funcName, params object[] args)
            {
                return null;
            }

            public virtual void RegisterHandlerTest(string funcName, Delegate handler)
            {
            }
        }

        Mock<IWebClient> mockWebClient;
        Connection connection;
        Mock<ConnectionImpl> mockConnectionImpl;

        [SetUp()]
        public void Init()
        {
            mockConnectionImpl = new Mock<ConnectionImpl> { CallBase = true };
            mockWebClient = new Mock<IWebClient>();
            connection = mockConnectionImpl.Object;
            connection.webClient = mockWebClient.Object;
        }

        [Test()]
        public void ShouldCallFunc()
        {
            var testFunc = connection.GenerateFuncWrapper("testFunc");
            var funcCall = testFunc("testArg", 42);
            mockConnectionImpl.Verify(c => c.CallFuncTest("testFunc", "testArg", 42), Times.Once());
        }

        [Test()]
        public void ShouldDownloadIDLAndCallProcessIDL()
        {
            mockWebClient.Setup(client => client.DownloadString(idlURL)).Returns("{}").Verifiable();
            connection.LoadIDL(idlURL);
            mockWebClient.Verify();
            mockConnectionImpl.Verify(c => c.ProcessIDLTest("{}"), Times.Once());
        }

        [Test()]
        public void ShouldCallRegisterHandler()
        {
            Delegate testDelegate = (Func<int, string>)delegate(int x) { return ""; };
            connection.RegisterFuncImplementation("testFunc", testDelegate);
            mockConnectionImpl.Verify(c => c.RegisterHandlerTest("testFunc", testDelegate), Times.Once());
        }


        [Test()]
        public void ShouldMapBracketOperatorToGenerateFuncWrapper()
        {
            connection["foo"](123);
            mockConnectionImpl.Verify(c => c.CallFuncTest("foo", 123), Times.Once());
        }

        // TODO: Tests for type mapping
    }
}

