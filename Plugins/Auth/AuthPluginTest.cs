using NUnit.Framework;
using System;

namespace Auth
{
    [TestFixture()]
    public class AuthPluginTest
    {
        AuthPlugin plugin = new AuthPlugin();

        [Test()]
        public void shouldReturnCorrectName()
        {
            Assert.AreEqual("Auth", plugin.getName());
        }

        [Test()]
        public void shouldReturnCorrectDeps()
        {
            Assert.AreEqual(plugin.getDependencies().Count, 0);
        }

        [Test()]
        public void shouldReturnLoginNameBySecurityToken()
        {
            var secToken = plugin.authenticate("test_user", "123");
            Assert.AreEqual("test_user", plugin.getLoginName(secToken));
        }

        [Test()]
        public void shouldAcceptAnyCombinationsOfLoginAndPassword()
        {
            Assert.AreNotEqual(Guid.Empty, plugin.authenticate("test_user", "123"));
            Assert.AreNotEqual(Guid.Empty, plugin.authenticate("test_user", "321"));
        }
    }
}

