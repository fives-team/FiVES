using System;
using NUnit.Framework;
using FIVES;

namespace Location
{
    [TestFixture()]
    public class LocationPluginTest
    {
        LocationPlugin plugin = new LocationPlugin();

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

