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
        Mock<Connection> connectionMock;

        [SetUp()]
        public void Init()
        {
            context = new Context();
            service = new ServiceImpl(context);
            connectionMock = new Mock<Connection>();
        }

        [Test()]
        public void ShouldInvokeOnNewClient()
        {
            int numConnectedClients =  0;
            service.OnNewClient += (c) => numConnectedClients++;
            service.HandleNewClient(connectionMock.Object);
            Assert.AreEqual(1, numConnectedClients);
        }

        [Test()]
        public void ShouldRegisterMethodsForNewConnections()
        {
            Delegate d1 = (Action) delegate {};
            Delegate d2 = (Func<string, int>) delegate(string arg) { return 42; };

            connectionMock.Setup(c => c.RegisterFuncImplementation("foobar", d1, "")).Verifiable();
            connectionMock.Setup(c => c.RegisterFuncImplementation("barfoo", d2, "")).Verifiable();

            service["foobar"] = d1;
            service["barfoo"] = d2;
            service.HandleNewClient(connectionMock.Object);

            connectionMock.VerifyAll();
        }
    }
}

