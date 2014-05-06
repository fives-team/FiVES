﻿// This file is part of FiVES.
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
using KIARAPlugin;
using Moq;
using NUnit.Framework;
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