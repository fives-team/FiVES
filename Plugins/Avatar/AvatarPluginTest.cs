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
            Assert.AreEqual("Avatar", plugin.getName());
        }

        [Test()]
        public void shouldReturnCorrectDeps()
        {
            Assert.AreEqual(plugin.getDependencies().Count, 2);
            Assert.Contains("ClientSync", plugin.getDependencies());
            Assert.Contains("Auth", plugin.getDependencies());
        }
    }
}