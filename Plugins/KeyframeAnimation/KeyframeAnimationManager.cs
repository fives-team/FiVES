using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventLoopPlugin;
using FIVES;

namespace KeyframeAnimationPlugin
{
    /// <summary>
    /// KeyframeAnimationManager maintains keyframe animations of entities for server side animation computation. KeyframeManager subscribes to EventLoop
    /// Plugin for recurring computation of animation keyframes
    /// </summary>
    internal class KeyframeAnimationManager
    {
        public KeyframeAnimationManager() {}

        internal void Initialize()
        {
            EventLoop.Instance.TickFired += new EventHandler<TickEventArgs>(HandleEventTick);
            LastTick = new TimeSpan(DateTime.Now.Ticks);
        }

        /// <summary>
        /// Handler for TickEvent of EventLoop. Will update keyframes of any animation and synchronize it with clients by setting the respective entity attribute
        /// </summary>
        /// <param name="sender">Sender of the event (EventLoop)</param>
        /// <param name="e">TickEvent args</param>
        private void HandleEventTick(Object sender, TickEventArgs e)
        {
            double frameDuration = e.TimeStamp.Subtract(LastTick).TotalMilliseconds;
            LastTick = e.TimeStamp;
            lock (SubscribedEntities)
            {
                foreach (KeyValuePair<String, Dictionary<string, KeyframeAnimation>> animatedEntities in SubscribedEntities)
                {
                    string animationKeyframes = "";
                    foreach (KeyValuePair<string, KeyframeAnimation> runningAnimation in animatedEntities.Value)
                    {
                        float newKey = PerformTickForEntityAnimation(animatedEntities.Key, runningAnimation.Value, frameDuration);
                        animationKeyframes += runningAnimation.Key + ":" + newKey + ";";
                    }
                    Entity entity = World.Instance.FindEntity(animatedEntities.Key);
                    entity["animation"]["animationKeyframes"] = animationKeyframes;
                }
            }
            FinalizeFinishedAnimations();
        }

        /// <summary>
        /// Computes the next frame for an animation of an entity. Stops the animation or increases cycle if frame range of animation is exceeded
        /// </summary>
        /// <param name="entityGuid">Guid of the entity for which the animation is computed</param>
        /// <param name="animation">Animation that is currently playing</param>
        /// <param name="frameDuration">Duration of the last frame in milliseconds</param>
        /// <returns>New Keyframe of the animation</returns>
        internal float PerformTickForEntityAnimation(string entityGuid, KeyframeAnimation animation, double frameDuration)
        {
            float newKey = 0f;

            if (!animation.Tick(frameDuration, out newKey)) // Perform next keyframe computation and stop animation if number of cycles reached the end
            {
                if (!FinishedAnimations.ContainsKey(entityGuid))
                    FinishedAnimations.Add(entityGuid, new HashSet<string>());

                FinishedAnimations[entityGuid].Add(animation.Name);
            }
            return newKey;
        }

        /// <summary>
        /// Stops all animations that exceeded their frame range and maximum number of cycles in the last frame
        /// </summary>
        internal void FinalizeFinishedAnimations()
        {
            foreach (KeyValuePair<string, HashSet<string>> finishedAnimationsForEntity in FinishedAnimations)
            {
                foreach (string animationGuid in finishedAnimationsForEntity.Value)
                    StopAnimation(finishedAnimationsForEntity.Key, animationGuid);
            }

            FinishedAnimations.Clear();
        }

        /// <summary>
        /// Starts an animation for a given entity
        /// </summary>
        /// <param name="entityGuid">Guid of entity for which animation should be played</param>
        /// <param name="animation">Keyframe animation that should be played for the entity</param>
        internal void StartAnimation(string entityGuid, KeyframeAnimation animation)
        {
            lock (SubscribedEntities)
            {
                if (!SubscribedEntities.ContainsKey(entityGuid))
                    SubscribedEntities[entityGuid] = new Dictionary<string, KeyframeAnimation>();

                if (!SubscribedEntities[entityGuid].ContainsKey(animation.Name))
                    SubscribedEntities[entityGuid].Add(animation.Name, animation);
                else
                    SubscribedEntities[entityGuid][animation.Name] = animation;
            }
        }

        /// <summary>
        /// Stops an animation of an entity, if playing
        /// </summary>
        /// <param name="entityGuid">Guid of the entity for which animation playback should be stopped</param>
        /// <param name="animationName">Name of the animation of which playback should be stopped</param>
        internal void StopAnimation(string entityGuid, string animationName)
        {
            lock (SubscribedEntities)
            {
                if (SubscribedEntities.ContainsKey(entityGuid))
                {
                    if(SubscribedEntities[entityGuid].ContainsKey(animationName))
                        SubscribedEntities[entityGuid].Remove(animationName);
                    if (SubscribedEntities[entityGuid].Count == 0)
                        SubscribedEntities.Remove(entityGuid);
                }
            }
        }

        /// <summary>
        /// Checks if a certain animation is currently playing for an entity
        /// </summary>
        /// <param name="entityGuid">Guid of entity to be checked</param>
        /// <param name="animationName">Name of animation for which playback should be checked</param>
        /// <returns></returns>
        public bool IsPlaying(string entityGuid, string animationName)
        {
            lock(SubscribedEntities)
                return SubscribedEntities.ContainsKey(entityGuid) && SubscribedEntities[entityGuid].ContainsKey(animationName);
        }

        internal Dictionary<string, Dictionary<string, KeyframeAnimation>> SubscribedEntities = new Dictionary<String, Dictionary<string, KeyframeAnimation>>();
        internal Dictionary<string, HashSet<string>> FinishedAnimations = new Dictionary<string, HashSet<string>>();
        private TimeSpan LastTick;
    }
}
