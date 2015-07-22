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

namespace KeyframeAnimationPlugin
{
    /// <summary>
    /// KeyAnimation class provides data and methods to manage server side animation processing.
    /// Animations are created when a server side animation is started.
    /// </summary>
    public class KeyframeAnimation
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
        /// Called cycle to increase keyframe of the animation depending on the duration of the last render frame
        /// in milliseconds
        /// </summary>
        /// <param name="frameDuration">Duration of the last frame in milliseconds</param>
        /// <param name="newFrame">Out parameter; contains the new keyframe of the animation after this tick</param>
        /// <returns>True, if animation is still playing, false, if animation stopped playing after this tick</returns>
        internal bool Tick(double frameDuration, out float newFrame)
        {
            CurrentFrame += Speed * (float)frameDuration / 1000f;
            if (CurrentFrame > EndFrame)
            {
                float frameRange = EndFrame - StartFrame;

                if(Cycles != -1) // For non-looping animation, check if animation has finished
                {
                    int numberOfSkippedCycles = (int)Math.Floor(CurrentFrame / frameRange);

                    CurrentCycle += numberOfSkippedCycles;
                    if (CurrentCycle > Cycles)
                    {
                        newFrame = EndFrame;
                        this.CurrentFrame = EndFrame;
                        return false;
                    }
                }

                // If CurrentFrame Exceeds endframe of current animation, resume it from start
                CurrentFrame = (StartFrame + (CurrentFrame - EndFrame)) % frameRange;
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
