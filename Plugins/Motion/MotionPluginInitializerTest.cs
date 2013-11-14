using System;
using NUnit.Framework;
using FIVES;

namespace MotionPlugin
{
    [TestFixture()]
    public class MotionPluginInitializerTest
    {
        MotionPluginInitializer plugin = new MotionPluginInitializer();

        public MotionPluginInitializerTest()
        {
            plugin.Initialize();
        }

        [Test()]
        public void ShouldRegisterVelocityComponent()
        {
            Assert.IsNotNull(ComponentRegistry.Instance.FindComponentDefinition("velocity"));
        }

        [Test()]
        public void ShouldRegisterRotVelocityComponent()
        {
            Assert.IsNotNull(ComponentRegistry.Instance.FindComponentDefinition("rotVelocity"));
        }

        [Test()]
        public void ShouldReturnCorrectName()
        {
            Assert.AreEqual("Motion", plugin.GetName());
        }

        [Test()]
        public void ShouldReturnCorrectDeps()
        {
            Assert.AreEqual(plugin.GetDependencies().Count, 1);
            Assert.Contains("Location", plugin.GetDependencies());
        }
    }
}

