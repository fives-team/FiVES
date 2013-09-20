using NUnit.Framework;
using System;

namespace Avatar
{
    [TestFixture()]
    public class AvatarPluginTest
    {
        AvatarPlugin plugin = new AvatarPlugin();

        [Test()]
        public void ShouldReturnCorrectName()
        {
            Assert.AreEqual("Avatar", plugin.GetName());
        }

        [Test()]
        public void ShouldReturnCorrectDeps()
        {
            Assert.AreEqual(plugin.GetDependencies().Count, 4);
            Assert.Contains("ClientManager", plugin.GetDependencies());
            Assert.Contains("Auth", plugin.GetDependencies());
            Assert.Contains("Renderable", plugin.GetDependencies());
            Assert.Contains("Location", plugin.GetDependencies());
        }
    }
}