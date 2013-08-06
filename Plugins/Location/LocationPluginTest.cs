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
            plugin.initialize();
        }

        [Test()]
        public void shouldRegisterPositionComponent()
        {
            Assert.True(ComponentRegistry.Instance.isRegistered("position"));
        }

        [Test()]
        public void shouldRegisterOrientationComponent()
        {
            Assert.True(ComponentRegistry.Instance.isRegistered("orientation"));
        }

        [Test()]
        public void shouldReturnCorrectNameAndDeps()
        {
            Assert.AreEqual("Location", plugin.getName());
            Assert.IsEmpty(plugin.getDependencies());
        }
    }
}

