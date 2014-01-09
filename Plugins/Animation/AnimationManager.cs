using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventLoopPlugin;
using FIVES;

namespace AnimationPlugin
{
    internal class AnimationManager
    {
        public AnimationManager()
        {
            EventLoop.Instance.TickFired += new EventHandler<TickEventArgs>(HandleEventTick);
            LastTick = new TimeSpan(DateTime.Now.Ticks);
        }

        private void HandleEventTick(Object sender, TickEventArgs e)
        {
            double frameDuration = e.TimeStamp.Subtract(LastTick).TotalMilliseconds;
            LastTick = e.TimeStamp;

            lock (SubscribedEntities)
            {
                foreach (KeyValuePair<String, Animation> registeredAnimation in SubscribedEntities)
                {
                    float newKey = registeredAnimation.Value.Tick(frameDuration);
                    Entity entity = World.Instance.FindEntity(registeredAnimation.Key);
                    entity["animation"]["keyframe"] = newKey;
                }
            }
        }

        internal void StartAnimation(string entityGuid, Animation animation)
        {
            lock(SubscribedEntities)
                SubscribedEntities[entityGuid] = animation;
        }

        internal void StopAnimation(string entityGuid, string animationName)
        {
            lock (SubscribedEntities)
            {
                if (SubscribedEntities.ContainsKey(entityGuid))
                    SubscribedEntities.Remove(entityGuid);
            }
        }

        public bool IsPlaying(string entityGuid, string animationName)
        {
            lock(SubscribedEntities)
                return SubscribedEntities.ContainsKey(entityGuid);
        }

        private Dictionary<String, Animation> SubscribedEntities = new Dictionary<String, Animation>();
        private TimeSpan LastTick;
    }
}
