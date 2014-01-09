using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnimationPlugin
{
    internal class Animation
    {
        public string Name;
        internal Animation(string name, float startFrame, float endFrame)
        {
            Name = name;
            StartFrame = startFrame;
            CurrentFrame = startFrame;
            EndFrame = endFrame;
        }

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
