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

        [Test()]
        public void ShouldApplyRotVelocityToOrientation()
        {
            var e = new Entity();
            Quat orientation = FIVES.Math.QuaternionFromAxisAngle(new Vector { x = 1, y = 0, z = 0 }, 0.1f);
            e["orientation"]["x"] = orientation.x;
            e["orientation"]["y"] = orientation.y;
            e["orientation"]["z"] = orientation.z;
            e["orientation"]["w"] = orientation.w;

            e["rotVelocity"]["x"] = 1f;
            e["rotVelocity"]["y"] = 0f;
            e["rotVelocity"]["z"] = 0f;
            e["rotVelocity"]["r"] = 0.1f;

            plugin.UpdateSpin(e);
            Quat entityOrientation = new Quat{ x = (float) e["orientation"]["x"],
                                               y = (float) e["orientation"]["y"],
                                               z = (float) e["orientation"]["z"],
                                               w = (float) e["orientation"]["w"]};

            Vector axisAfterSpin = FIVES.Math.AxisFromQuaternion(entityOrientation);
            float angleAfterSpin = FIVES.Math.AngleFromQuaternion(entityOrientation);

            Assert.Less(System.Math.Abs(1f - axisAfterSpin.x), 0.00001f);
            Assert.Less(System.Math.Abs(axisAfterSpin.y), 0.00001f);
            Assert.Less(System.Math.Abs(axisAfterSpin.z), 0.00001f);
            Assert.Less(System.Math.Abs(0.2f-angleAfterSpin), 0.00001f);
        }
    }
}

