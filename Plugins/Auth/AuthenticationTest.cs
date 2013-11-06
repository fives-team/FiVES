using NUnit.Framework;
using System;

namespace AuthPlugin
{
    [TestFixture()]
    public class AuthenticationTest
    {
        Authentication authentication = new Authentication();

        [Test()]
        public void ShouldReturnLoginNameBySecurityToken()
        {
            var secToken = authentication.Authenticate("test_user", "123");
            Assert.AreEqual("test_user", authentication.GetLoginName(secToken));
        }

        [Test()]
        public void ShouldAcceptAnyCombinationsOfLoginAndPassword()
        {
            Assert.AreNotEqual(Guid.Empty, authentication.Authenticate("test_user", "123"));
            Assert.AreNotEqual(Guid.Empty, authentication.Authenticate("test_user", "321"));
        }
    }
}