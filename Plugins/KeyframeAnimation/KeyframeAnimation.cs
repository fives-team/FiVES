using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeyframeAnimationPlugin
{
    /// <summary>
    /// KeyAnimation class provides data and methods to manage server side animation processing.
    /// Animations are created when a server side animation is started.
    /// </summary>
    internal class KeyframeAnimation
    {
        /// <summary>
        /// Name of the animation for identification
        /// </summary>
        public string Name;

        internal KeyframeAnimation(string name, float startFrame, float endFrame, int cycles, float speed)
        {
            Name = name;
            StartFrame = startFrame;
            CurrentFrame = startFrame;
            EndFrame = endFrame;
            Cycles = cycles;
            CurrentCycle = 1;
            Speed = speed;
        }

        /// <summary>
        /// Called cycle to increase keyframe of the animation depending on the duration of the last render frame in milliseconds
        /// </summary>
        /// <param name="frameDuration">Duration of the last frame in milliseconds</param>
        /// <param name="newFrame">Out parameter; contains the new keyframe of the animation after this tick</param>
        /// <returns>True, if animation is still playing, false, if animation stopped playing after this tick</returns>
        internal bool Tick(double frameDuration, out float newFrame)
        {
            CurrentFrame += Speed * (float)frameDuration / 1000f;
            if (CurrentFrame > EndFrame)
            {
                if(Cycles != -1)
                {
                    CurrentCycle ++;
                    if (CurrentCycle > Cycles)
                    {
                        newFrame = EndFrame;
                        this.CurrentFrame = EndFrame;
                        return false;
                    }
                }
                float frameRange = EndFrame - StartFrame;
                CurrentFrame = (StartFrame + (CurrentFrame - EndFrame)) % frameRange; // If CurrentFrame Exceeds endframe of current animation, resume it from start
            }
            newFrame = CurrentFrame;
            return true;
        }

        internal float StartFrame;
        internal float EndFrame;
        internal float CurrentFrame;

        internal int Cycles;
        internal int CurrentCycle;
        internal float Speed;
    }
}
