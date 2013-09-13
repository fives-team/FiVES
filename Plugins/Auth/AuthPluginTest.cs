using NUnit.Framework;
using System;

namespace Auth
{
    [TestFixture()]
    public class AuthPluginTest
    {
        AuthPlugin plugin = new AuthPlugin();

        [Test()]
        public void ShouldReturnCorrectName()
        {
            Assert.AreEqual("Auth", plugin.GetName());
        }

        [Test()]
        public void ShouldReturnCorrectDeps()
        {
            Assert.AreEqual(plugin.GetDependencies().Count, 1);
            Assert.Contains("DirectCall", plugin.GetDependencies());
        }

        [Test()]
        public void ShouldReturnLoginNameBySecurityToken()
        {
            var secToken = plugin.Authenticate("test_user", "123");
            Assert.AreEqual("test_user", plugin.getLoginName(secToken));
        }

        [Test()]
        public void ShouldAcceptAnyCombinationsOfLoginAndPassword()
        {
            Assert.AreNotEqual(Guid.Empty, plugin.Authenticate("test_user", "123"));
            Assert.AreNotEqual(Guid.Empty, plugin.Authenticate("test_user", "321"));
        }
    }
}

