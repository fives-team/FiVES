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
    public class ProtocolRegistryTest
    {
        private ProtocolRegistry protocolRegistry;

        [SetUp()]
        public void Init()
        {
            protocolRegistry = new ProtocolRegistry();
        }

        [Test()]
        public void ShouldRegisterNewProtocols()
        {
            var connectionFactory = new Mock<IConnectionFactory>();
            protocolRegistry.RegisterConnectionFactory("test", connectionFactory.Object);
            Assert.True(protocolRegistry.IsRegistered("test"));
            Assert.AreEqual(protocolRegistry.GetConnectionFactory("test"), connectionFactory.Object);
        }

        public void ShouldThrowExceptionWhenAskingForNonRegisteredProtocol()
        {
            Assert.False(protocolRegistry.IsRegistered("unregistered-protocol"));
            Assert.Throws<Error>(() => protocolRegistry.GetConnectionFactory("unregistered-protocol"));
        }
    }
}

