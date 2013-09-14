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
        public void shouldRegisterPositionComponent()
        {
            Assert.True(ComponentRegistry.Instance.IsRegistered("position"));
        }

        [Test()]
        public void shouldRegisterOrientationComponent()
        {
            Assert.True(ComponentRegistry.Instance.IsRegistered("orientation"));
        }

        [Test()]
        public void shouldReturnCorrectName()
        {
            Assert.AreEqual("Location", plugin.GetName());
        }

        [Test()]
        public void shouldReturnCorrectDeps()
        {
            Assert.IsEmpty(plugin.GetDependencies());
        }
    }
}

