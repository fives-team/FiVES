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