using System;
using NUnit.Framework;
using FIVES;

namespace MotionPlugin
{
    [TestFixture()]
    public class MotionPluginInitializerTest
    {
        MotionPluginInitializer plugin = new MotionPluginInitializer();

        ComponentRegistry globalComponentRegistry = ComponentRegistry.Instance;

        [SetUp()]
        public void Init()
        {
            ComponentRegistry.Instance = new ComponentRegistry();
            plugin.RegisterToECA();
            RegisterLocationComponents();
        }
        }

        [TearDown()]
        public void ShutDown()
        {
            ComponentRegistry.Instance = globalComponentRegistry;
        }

        [Test()]
        [Ignore()]
        public void ShouldRegisterVelocityComponent()
        {
            Assert.IsNotNull(ComponentRegistry.Instance.FindComponentDefinition("velocity"));
        }

        [Test()]
        [Ignore()]
        public void ShouldRegisterRotVelocityComponent()
        {
            Assert.IsNotNull(ComponentRegistry.Instance.FindComponentDefinition("rotVelocity"));
        }
    }
}

