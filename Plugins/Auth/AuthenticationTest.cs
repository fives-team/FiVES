// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using Moq;
using NUnit.Framework;
using SINFONI;
using System;

namespace AuthPlugin
{
    [TestFixture()]
    public class AuthenticationTest
    {
        Authentication authentication = new Authentication();
        Mock<Connection> mockConnection = new Mock<Connection>();

        [Test()]
        public void ShouldReturnLoginNameBySecurityToken()
        {
            Assert.IsTrue(authentication.Authenticate(mockConnection.Object, "test_user", "123"));
            Assert.AreEqual("test_user", authentication.GetLoginName(mockConnection.Object));
        }

        [Test()]
        public void ShouldAcceptAnyCombinationsOfLoginAndPassword()
        {
            Assert.IsTrue(authentication.Authenticate(mockConnection.Object, "test_user", "123"));
            Assert.IsTrue(authentication.Authenticate(mockConnection.Object, "test_user", "321"));
        }
    }
}