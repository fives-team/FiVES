using NUnit.Framework;
using System;

namespace Avatar
{
    [TestFixture()]
    public class AvatarPluginTest
    {
        AvatarPlugin plugin = new AvatarPlugin();

        [Test()]
        public void shouldReturnCorrectName()
        {
            Assert.AreEqual("Avatar", plugin.GetName());
        }

        [Test()]
        public void shouldReturnCorrectDeps()
        {
            Assert.AreEqual(plugin.GetDependencies().Count, 3);
            Assert.Contains("ClientSync", plugin.GetDependencies());
            Assert.Contains("Auth", plugin.GetDependencies());
            Assert.Contains("DirectCall", plugin.GetDependencies());
        }
    }
}