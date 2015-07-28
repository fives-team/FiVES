// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventLoopPlugin;
using FIVES;

namespace KeyframeAnimationPlugin
{
    /// <summary>
    /// KeyframeAnimationManager maintains keyframe animations of entities for server side animation computation.
    /// KeyframeManager subscribes to EventLoopPlugin for recurring computation of animation keyframes
    /// </summary>
    public class KeyframeAnimationManager
    {
        public static KeyframeAnimationManager Instance;

        internal KeyframeAnimationManager() {}

        internal void Initialize()
        {
            EventLoop.Instance.TickFired += new EventHandler<TickEventArgs>(HandleEventTick);
            LastTick = new TimeSpan(DateTime.Now.Ticks);
        }

        internal void Initialize(KeyframeAnimationPluginInitializer plugin)
        {
            this.animationPlugin = plugin;
            Initialize();
        }

        /// <summary>
        /// Handler for TickEvent of EventLoop. Will update keyframes of any animation and synchronize it with
        /// clients by setting the respective entity attribute
        /// </summary>
        /// <param name="sender">Sender of the event (EventLoop)</param>
        /// <param name="e">TickEvent args</param>
        private void HandleEventTick(Object sender, TickEventArgs e)
        {
            double frameDuration = e.TimeStamp.Subtract(LastTick).TotalMilliseconds;
            LastTick = e.TimeStamp;
            lock (RunningAnimationsForEntities)
            {
                foreach (KeyValuePair<Guid, Dictionary<string, KeyframeAnimation>>
                    animatedEntity in RunningAnimationsForEntities)
                {
                    string animationKeyframes = "";
                    foreach (KeyValuePair<string, KeyframeAnimation> runningAnimation in animatedEntity.Value)
                    {
                        float newKey = PerformTickForEntityAnimation(animatedEntity.Key,
                                                                     runningAnimation.Value,
                                                                     frameDuration);

                        animationKeyframes += runningAnimation.Key + ":" + newKey + ";";
                    }
                    Entity entity = World.Instance.FindEntity(animatedEntity.Key);
                    entity["animation"]["animationKeyframes"].Suggest(animationKeyframes);
                }
            }
            FinalizeFinishedAnimations();
        }

        /// <summary>
        /// Computes the next frame for an animation of an entity. Stops the animation or increases cycle if
        /// frame range of animation is exceeded
        /// </summary>
        /// <param name="entityGuid">Guid of the entity for which the animation is computed</param>
        /// <param name="animation">Animation that is currently playing</param>
        /// <param name="frameDuration">Duration of the last frame in milliseconds</param>
        /// <returns>New Keyframe of the animation</returns>
        internal float PerformTickForEntityAnimation(Guid entityGuid, KeyframeAnimation animation, double frameDuration)
        {
            float newKey = 0f;

            // Perform next keyframe computation and stop animation if number of cycles reached the end
            if (!animation.Tick(frameDuration, out newKey))
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
            foreach (KeyValuePair<Guid, HashSet<string>> finishedAnimationsForEntity in FinishedAnimations)
            {
                foreach (string animationName in finishedAnimationsForEntity.Value)
                    StopAnimation(finishedAnimationsForEntity.Key, animationName);
            }

            FinishedAnimations.Clear();
        }

        /// <summary>
        /// Starts an animation for a given entity
        /// </summary>
        /// <param name="entityGuid">Guid of entity for which animation should be played</param>
        /// <param name="animation">Keyframe animation that should be played for the entity</param>
        public void StartAnimation(Guid entityGuid, KeyframeAnimation animation)
        {
            lock (RunningAnimationsForEntities)
            {
                if (!RunningAnimationsForEntities.ContainsKey(entityGuid))
                    RunningAnimationsForEntities[entityGuid] = new Dictionary<string, KeyframeAnimation>();

                if (!RunningAnimationsForEntities[entityGuid].ContainsKey(animation.Name))
                    RunningAnimationsForEntities[entityGuid].Add(animation.Name, animation);
                else
                    RunningAnimationsForEntities[entityGuid][animation.Name] = animation;
            }
        }

        /// <summary>
        /// Stops an animation of an entity, if playing
        /// </summary>
        /// <param name="entityGuid">Guid of the entity for which animation playback should be stopped</param>
        /// <param name="animationName">Name of the animation of which playback should be stopped</param>
        public void StopAnimation(Guid entityGuid, string animationName)
        {
            lock (RunningAnimationsForEntities)
            {
                if (RunningAnimationsForEntities.ContainsKey(entityGuid))
                {
                    if (RunningAnimationsForEntities[entityGuid].ContainsKey(animationName))
                        RunningAnimationsForEntities[entityGuid].Remove(animationName);
                    if (RunningAnimationsForEntities[entityGuid].Count == 0)
                        RunningAnimationsForEntities.Remove(entityGuid);
                }
            }
        }

        /// <summary>
        /// Checks if a certain animation is currently playing for an entity
        /// </summary>
        /// <param name="entityGuid">Guid of entity to be checked</param>
        /// <param name="animationName">Name of animation for which playback should be checked</param>
        /// <returns></returns>
        public bool IsPlaying(Guid entityGuid, string animationName)
        {
            lock(RunningAnimationsForEntities)
                return RunningAnimationsForEntities.ContainsKey(entityGuid)
                    && RunningAnimationsForEntities[entityGuid].ContainsKey(animationName);
        }

        /// <summary>
        /// SINFONI Service method handler that initiates a server side animation playback for an entity
        /// </summary>
        /// <param name="entityGuid">Guid of entity for which animation should be played</param>
        /// <param name="name">Name of animation that should be played</param>
        /// <param name="startFrame">Keyframe at which animation playback should start</param>
        /// <param name="endFrame">Keyframe at which animation playback should end</param>
        /// <param name="cycles">Number of cycles the animation should be played (-1 for infinite playback)</param>
        /// <param param name="speed">Speed in which animation should be played</param>
        public void StartServersideAnimation(Guid entityGuid,
            string animationName,
            float startFrame,
            float endFrame,
            int cycles,float speed)
        {
            animationPlugin.StartServersideAnimation(entityGuid.ToString(), animationName, startFrame, endFrame, cycles, speed);
        }

        /// <summary>
        /// Invokes a message to start animations on clients' sides
        /// </summary>
        /// <param name="entityGuid">Guid of entity for which animation should be played</param>
        /// <param name="name">Name of animation that should be played</param>
        /// <param name="startFrame">Keyframe at which animation playback should start</param>
        /// <param name="endFrame">Keyframe at which animation playback should end</param>
        /// <param name="cycles">Number of cycles the animation should be played (-1 for infinite playback)</param>
        /// <param param name="speed">Speed in which animation should be played</param>
        public void StartClientsideAnimation(Guid entityGuid,
            string animationName,
            float startFrame,
            float endFrame,
            int cycles,float speed)
        {
            animationPlugin.StartClientsideAnimation(entityGuid.ToString(), animationName, startFrame, endFrame, cycles, speed);
        }

        /// <summary>
        /// Handler that stops a server side animation playback for an entity
        /// </summary>
        /// <param name="entityGuid">Guid for which playback should be stopped</param>
        /// <param name="name">Name of animation for which playback should be stopped</param>
        public void StopServersideAnimation(Guid entityGuid, string name)
        {
            animationPlugin.StopServersideAnimation(entityGuid.ToString(), name);
        }

        /// <summary>
        /// Invokes a message to stop animations on clients' sides
        /// </summary>
        /// <param name="entityGuid">Guid of entity for which animation should be played</param>
        /// <param name="name">Name of animation that should be played</param>
        public void StopClientsideAnimation(Guid entityGuid, string name)
        {
            animationPlugin.StopClientsideAnimation(entityGuid.ToString(), name);
        }
        /// <summary>
        /// Registry of all entities that subscribed to receive tick events from event loop for animation
        /// key frame computation.
        /// Dictionary has stucture Dictionary<EntityGuid,Dictionary<AnimationName, AnimationObject>,
        /// e.g. Dict[1]["walk"] = animation object for walk animation of entity 1
        /// </summary>
        internal Dictionary<Guid, Dictionary<string, KeyframeAnimation>> RunningAnimationsForEntities =
            new Dictionary<Guid, Dictionary<string, KeyframeAnimation>>();

        /// <summary>
        /// Registry of all animations that finished playback in last frame. After frame tick has finished,
        /// i.e. all running animations have computed their new frame, finished animations are removed from
        /// the registry of running animations. If an entity has finished playback of all its animations,
        /// it is removed from the registry completely
        /// </summary>
        internal Dictionary<Guid, HashSet<string>> FinishedAnimations = new Dictionary<Guid, HashSet<string>>();
        private TimeSpan LastTick;

        KeyframeAnimationPluginInitializer animationPlugin;
    }
}
