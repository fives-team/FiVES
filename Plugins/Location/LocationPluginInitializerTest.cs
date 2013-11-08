using System;
using NUnit.Framework;
using FIVES;

namespace LocationPlugin
{
    [TestFixture()]
    public class LocationPluginInitializerTest
    {
        LocationPluginInitializer plugin = new LocationPluginInitializer();

        public LocationPluginInitializerTest()
        {
            plugin.Initialize();
        }

        [Test()]
        public void ShouldRegisterPositionComponent()
        {
            Assert.IsNotNull(ComponentRegistry.Instance.FindComponentDefinition("position"));
        }

        [Test()]
        public void ShouldRegisterOrientationComponent()
        {
            Assert.IsNotNull(ComponentRegistry.Instance.FindComponentDefinition("orientation"));
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

