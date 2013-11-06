using System;
using NUnit.Framework;
using FIVES;

namespace LocationPlugin
{
    [TestFixture()]
    public class LocationPluginTest
    {
        LocationPluginInitializer plugin = new LocationPluginInitializer();

        public LocationPluginTest()
        {
            plugin.Initialize();
        }

        [Test()]
        public void ShouldRegisterPositionComponent()
        {
            Assert.True(ComponentRegistry.Instance.IsRegistered("position"));
        }

        [Test()]
        public void ShouldRegisterOrientationComponent()
        {
            Assert.True(ComponentRegistry.Instance.IsRegistered("orientation"));
        }

        [Test()]
        public void ShouldReturnCorrectName()
        {
            Assert.AreEqual("Location", plugin.GetName());
        }

        [Test()]
        public void ShouldReturnCorrectDeps()
        {
            Assert.IsEmpty(plugin.GetDependencies());
        }
    }
}

