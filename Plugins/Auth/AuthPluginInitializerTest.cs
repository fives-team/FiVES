using NUnit.Framework;
using System;

namespace AuthPlugin
{
    [TestFixture()]
    public class AuthPluginInitializerTest
    {
        AuthPluginInitializer plugin = new AuthPluginInitializer();

        [Test()]
        public void ShouldReturnCorrectName()
        {
            Assert.AreEqual("Auth", plugin.GetName());
        }

        [Test()]
        public void ShouldReturnCorrectDeps()
        {
            Assert.AreEqual(plugin.GetDependencies().Count, 0);
        }
    }
}

