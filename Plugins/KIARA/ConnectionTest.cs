// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
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
#pragma warning disable 67
            public override event EventHandler Closed;
#pragma warning restore 67

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
            testFunc("testArg", 42);
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

