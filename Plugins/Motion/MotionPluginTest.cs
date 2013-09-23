using System;
using NUnit.Framework;
using FIVES;

namespace Motion
{
    [TestFixture()]
    public class MotionPluginTest
    {
        MotionPlugin plugin = new MotionPlugin();

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
            Assert.IsEmpty(plugin.GetDependencies());
        }
    }
}

