using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;
using NUnit.Framework;

namespace KeyframeAnimationPlugin
{
    [TestFixture()]
    class AnimationPluginTest
    {

        private KeyframeAnimationPluginInitializer plugin;
        private string createdEntityGuid;
        private Guid entityGuidAsGuid;

        /// <summary>
        /// Creates a plugin and registers its components. This skips initialization of the KeyframeAnimationManager and
        /// registering to the EventLoop.
        /// Creates one single entity to test animations on
        /// </summary>
        public AnimationPluginTest()
        {
            plugin = new KeyframeAnimationPluginInitializer();
            plugin.RegisterComponents();
            Entity entity = new Entity();
            createdEntityGuid = entity.Guid.ToString();
            entityGuidAsGuid = entity.Guid;
            World.Instance.Add(entity);
        }

        /// <summary>
        /// Start with clean registry for every test
        /// </summary>
        [SetUp()]
        public void InitializeTest()
        {
            plugin.Manager.RunningAnimationsForEntities.Clear();
        }

        /// <summary>
        /// Tests if an entry for the animated entity is created in the respective registry
        /// </summary>
        [Test()]
        public void ManagerShouldRegisterEntityForServersideAnimation()
        {
            plugin.StartServersideAnimation(createdEntityGuid, "testAnimation", 0f, 1f, 1, 1f);
            Assert.Contains(entityGuidAsGuid, plugin.Manager.RunningAnimationsForEntities.Keys);
        }

        /// <summary>
        /// Tests if the correct animation object is created after a serverside animation start was invoked
        /// </summary>
        [Test()]
        public void ManagerShouldRegisterAnimationForServersideAnimation()
        {
            plugin.StartServersideAnimation(createdEntityGuid, "testAnimation", 0f, 1f, 1, 1f);
            Dictionary<string, KeyframeAnimation> animations = plugin.Manager.RunningAnimationsForEntities[entityGuidAsGuid];

            Assert.Contains("testAnimation", animations.Keys);

            KeyframeAnimation registeredAnimation = animations["testAnimation"];
            Assert.AreEqual(registeredAnimation.StartFrame, 0f);
            Assert.AreEqual(registeredAnimation.EndFrame, 1f);
            Assert.AreEqual(registeredAnimation.Cycles, 1);
            Assert.AreEqual(registeredAnimation.Speed, 1f);
        }

        /// <summary>
        /// Tests whether an "AnimationStop" command from the plugin removes an animation from the registry of running animation
        /// </summary>
        [Test()]
        public void ManagerShouldRemoveAnimationOnStop()
        {
            plugin.StartServersideAnimation(createdEntityGuid, "testAnimation", 0f, 1f, 1, 1f);
            plugin.StartServersideAnimation(createdEntityGuid, "testAnimation2", 0f, 1f, 1, 1f);
            plugin.StopServersideAnimation(createdEntityGuid, "testAnimation");
            Assert.False(plugin.Manager.RunningAnimationsForEntities[entityGuidAsGuid].Keys.Contains("testAnimation"));
            Assert.AreEqual(plugin.Manager.RunningAnimationsForEntities[entityGuidAsGuid].Keys.Count, 1);
        }
        /// <summary>
        /// Tests whether the tick function increases the current frame by the correct value
        /// </summary>
        [Test()]
        public void AnimationShouldPerformCorrectAnimationTick()
        {
            float newFrame;
            KeyframeAnimation animation = new KeyframeAnimation("testAnimation", 0f, 1f, 1, 1f);
            animation.Tick(500, out newFrame);

            Assert.AreEqual(animation.CurrentFrame, 0.5f);
            Assert.AreEqual(newFrame, animation.CurrentFrame);
        }

        /// <summary>
        /// Tests whether the number of cycles is increased correctly when an animation exceeds its frame limit
        /// </summary>
        [Test()]
        public void AnimationShouldIncreaseCycle()
        {
            float newFrame;
            KeyframeAnimation animation = new KeyframeAnimation("testAnimation", 0f, 1f, 2, 1f);
            animation.Tick(1500, out newFrame);

            Assert.AreEqual(animation.CurrentFrame, 0.5f);
            Assert.AreEqual(newFrame, animation.CurrentFrame);
            Assert.AreEqual(animation.CurrentCycle, 2);
        }

        /// <summary>
        /// Very low frame rates and thus high frame durations may occur in situations when the adapted keyframe in the new cycle
        /// still lies outside the frame range. This test checks whether this case is handled correctly.
        /// </summary>
        [Test()]
        public void AnimationShouldNotSkipCycles()
        {
            float newFrame;
            KeyframeAnimation animation = new KeyframeAnimation("testAnimation", 0f, 1f, 2, 1f);
            animation.Tick(4500, out newFrame);
            Assert.AreEqual(animation.CurrentFrame, 0.5f);
            Assert.AreEqual(newFrame, animation.CurrentFrame);
            Assert.AreEqual(animation.CurrentCycle, 2);
        }

        /// <summary>
        /// Tests whether animations that exceeded their frame ranges and their maximum number of cycles are correctly marked to be stopped in the next frame
        /// </summary>
        [Test()]
        public void ManagerShouldRegisterFinishedAnimationAsFinished()
        {
            KeyframeAnimation animation = new KeyframeAnimation("testAnimation", 0f, 1f, 1, 1f);
            plugin.Manager.PerformTickForEntityAnimation(entityGuidAsGuid, animation, 1500);
            Assert.True(plugin.Manager.FinishedAnimations.ContainsKey(entityGuidAsGuid));
            Assert.True(plugin.Manager.FinishedAnimations[entityGuidAsGuid].Contains("testAnimation"));
        }

        /// <summary>
        /// Tests if the entry for a finished animation is correctly removed from the list of registered animations without interferring with other registered
        /// animations
        /// </summary>
        [Test()]
        public void ManagerShouldRemoveFinishedAnimations()
        {
            KeyframeAnimation animation1 = new KeyframeAnimation("testAnimation1", 0f, 1f, 1, 1f);
            KeyframeAnimation animation2 = new KeyframeAnimation("testAnimation2", 0f, 1f, 1, 1f);
            plugin.Manager.StartAnimation(entityGuidAsGuid, animation1);
            plugin.Manager.StartAnimation(entityGuidAsGuid, animation2);
            plugin.Manager.PerformTickForEntityAnimation(entityGuidAsGuid, animation1, 1500);
            plugin.Manager.FinalizeFinishedAnimations();
            Assert.Contains("testAnimation2", plugin.Manager.RunningAnimationsForEntities[entityGuidAsGuid].Keys);
            Assert.False(plugin.Manager.RunningAnimationsForEntities[entityGuidAsGuid].Keys.Contains("testAnimation1"));
        }

        /// <summary>
        /// Tests if an entity is removed from the registry for animated entities if the last animation of the entity did stop
        /// </summary>
        [Test()]
        public void ManagerShouldRemoveFinishedAnimatedEntities()
        {
            KeyframeAnimation animation = new KeyframeAnimation("testAnimation", 0f, 1f, 1, 1f);
            plugin.Manager.StartAnimation(entityGuidAsGuid, animation);
            plugin.Manager.PerformTickForEntityAnimation(entityGuidAsGuid, animation, 1500);
            plugin.Manager.FinalizeFinishedAnimations();
            Assert.IsEmpty(plugin.Manager.RunningAnimationsForEntities);
        }

        /// <summary>
        /// Tests if starting an animation for same entity with same name but new paraemeters again replaces the old parameterlist
        /// </summary>
        [Test()]
        public void ManagerShouldReplaceValuesOnNewAnimationStart()
        {
            plugin.StartServersideAnimation(createdEntityGuid, "testAnimation", 0f, 1f, 1, 1f);
            plugin.StartServersideAnimation(createdEntityGuid, "testAnimation", 0.5f, 1.5f, 2, 2f);

            Assert.True(plugin.Manager.RunningAnimationsForEntities.ContainsKey(entityGuidAsGuid));
            KeyframeAnimation testAnimation = plugin.Manager.RunningAnimationsForEntities[entityGuidAsGuid]["testAnimation"];
            Assert.AreEqual(1, plugin.Manager.RunningAnimationsForEntities[entityGuidAsGuid].Keys.Count);
            Assert.AreEqual(testAnimation.StartFrame, 0.5f);
            Assert.AreEqual(testAnimation.EndFrame, 1.5f);
            Assert.AreEqual(testAnimation.Cycles, 2);
            Assert.AreEqual(testAnimation.Speed, 2f);
        }
    }
}
