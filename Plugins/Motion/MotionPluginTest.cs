using System;
using NUnit.Framework;
using FIVES;

namespace MotionPlugin
{
    [TestFixture()]
    public class MotionPluginTest
    {
        MotionPluginInitializer plugin = new MotionPluginInitializer();

        public MotionPluginTest()
        {
            plugin.Initialize();
        }

        [Test()]
        public void ShouldRegisterVelocityComponent()
        {
            Assert.True(ComponentRegistry.Instance.IsRegistered("velocity"));
        }

        [Test()]
        public void ShouldRegisterRotVelocityComponent()
        {
            Assert.True(ComponentRegistry.Instance.IsRegistered("rotVelocity"));
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

