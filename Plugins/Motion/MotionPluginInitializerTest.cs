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

        private void RegisterLocationComponents()
        {
            var positionComponent = new ComponentDefinition("position");
            positionComponent.AddAttribute<float>("x", 0f);
            positionComponent.AddAttribute<float>("y", 0f);
            positionComponent.AddAttribute<float>("z", 0f);
            ComponentRegistry.Instance.Register(positionComponent);

            var orientationComponent = new ComponentDefinition("orientation");
            orientationComponent.AddAttribute<float>("x", 0f);
            orientationComponent.AddAttribute<float>("y", 0f);
            orientationComponent.AddAttribute<float>("z", 0f);
            orientationComponent.AddAttribute<float>("w", 1f);
            ComponentRegistry.Instance.Register(orientationComponent);

        }

        [TearDown()]
        public void ShutDown()
        {
            ComponentRegistry.Instance = globalComponentRegistry;
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
        public void ShouldApplyVelocityToPosition()
        {
            var e = new Entity();
            e["velocity"]["x"] = 1f;

            plugin.UpdateMotion(e);
            Assert.AreEqual((float)e["position"]["x"], 1f);
            Assert.AreEqual((float)e["position"]["y"], 0f);
            Assert.AreEqual((float)e["position"]["z"], 0f);
        }

    }
}

