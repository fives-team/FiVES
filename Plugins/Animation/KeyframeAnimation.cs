using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnimationPlugin
{
    /// <summary>
    /// KeyAnimation class provides data and methods to manage server side animation processing. Animations are created when a server side animation is started.
    /// </summary>
    internal class KeyframeAnimation
    {
        /// <summary>
        /// Name of the animation for identification
        /// </summary>
        public string Name;

        internal KeyframeAnimation(string name, float startFrame, float endFrame)
        {
            Name = name;
            StartFrame = startFrame;
            CurrentFrame = startFrame;
            EndFrame = endFrame;
        }

        /// <summary>
        /// Called cycle to increase keyframe of the animation depending on the duration of the last render frame in milliseconds
        /// </summary>
        /// <param name="frameDuration">Duration of the last frame in milliseconds</param>
        /// <returns>The new keyframe for the animation</returns>
        internal float Tick(double frameDuration)
        {
            CurrentFrame += (float)frameDuration / 1000f;
            if (CurrentFrame > EndFrame)
            {
                CurrentFrame = StartFrame + (CurrentFrame - EndFrame); // If CurrentFrame Exceeds endframe of current animation, resume it from start
            }
            return CurrentFrame;
        }

        private float StartFrame;
        private float EndFrame;
        private float CurrentFrame;
    }
}
