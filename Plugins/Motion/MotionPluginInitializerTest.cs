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

        /// <summary>
        /// Plugin and component dependencies are not resolved when using plugins in Unit tests.
        /// Therefore, we need to register the components of Location plugin manually
        /// </summary>
        private void RegisterLocationComponents()
        {
            ComponentDefinition locationComponent = new ComponentDefinition("location");
            locationComponent.AddAttribute<Vector>("position", new Vector(0, 0, 0));
            locationComponent.AddAttribute<Quat>("orientation", new Quat(0, 0, 0, 1));
            ComponentRegistry.Instance.Register(locationComponent);
        }

        [TearDown()]
        public void ShutDown()
        {
            ComponentRegistry.Instance = globalComponentRegistry;
        }

        [Test()]
        public void ShouldRegisterMotionComponent()
        {
            Assert.IsNotNull(ComponentRegistry.Instance.FindComponentDefinition("motion"));
        }

        /// <summary>
        /// Tests if the velocity values are applied correctly to an entity's position.
        /// Update computation is invoked manually once (usually done after TickFired
        /// event of eventloop). Expected outcome is position increased by velocity
        /// </summary>
        [Test()]
        public void ShouldApplyVelocityToPosition()
        {
            var e = new Entity();
            e["motion"]["velocity"] = new Vector(1, 0, 0);

            plugin.UpdateMotion(e);

            Vector newPosition = (Vector)e["location"]["position"];

            Assert.AreEqual(newPosition.x, 1f);
            Assert.AreEqual(newPosition.y, 0f);
            Assert.AreEqual(newPosition.z, 0f);
        }

        /// <summary>
        /// Tests if rotational velocity is applied correctly to an entities orientation.
        /// Update function is called manually once (usually done after TickFired event
        /// of EventLoop).
        /// Attention: Frequent conversion from Axis-Angle to Quaternion with Single
        /// floating point precision introduces floating point inaccuracies, so we
        /// do not test for equality of expected outcome, but if expected outcome is in
        /// acceptable range.
        /// </summary>
        [Test()]
        public void ShouldApplyRotVelocityToOrientation()
        {
            var e = new Entity();
            Quat orientation = FIVES.Math.QuaternionFromAxisAngle(new Vector(1, 0, 0), 0.1f);

            e["location"]["orientation"] = orientation;
            e["motion"]["rotVelocity"] = new AxisAngle(1f, 0f, 0f, 0.1f);

            plugin.UpdateSpin(e);

            Quat newEntityOrientation = (Quat)e["location"]["orientation"];

            Vector axisAfterSpin = FIVES.Math.AxisFromQuaternion(newEntityOrientation);
            float angleAfterSpin = FIVES.Math.AngleFromQuaternion(newEntityOrientation);

            Assert.Less(System.Math.Abs(1f - axisAfterSpin.x), 0.00001f);
            Assert.Less(System.Math.Abs(axisAfterSpin.y), 0.00001f);
            Assert.Less(System.Math.Abs(axisAfterSpin.z), 0.00001f);
            Assert.Less(System.Math.Abs(0.2f-angleAfterSpin), 0.00001f);
        }
    }
}

