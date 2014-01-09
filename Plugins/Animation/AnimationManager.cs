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
                foreach (KeyValuePair<String, Dictionary<string, Animation>> animatedEntities in SubscribedEntities)
                {
                    string animationKeyframes = "";
                    foreach (KeyValuePair<string, Animation> runningAnimation in animatedEntities.Value)
                    {
                        float newKey = runningAnimation.Value.Tick(frameDuration);
                        animationKeyframes += runningAnimation.Key + ":" + newKey + ";";
                    }
                    Entity entity = World.Instance.FindEntity(animatedEntities.Key);
                    entity["animation"]["animationKeyframes"] = animationKeyframes;
                }
            }
        }

        internal void StartAnimation(string entityGuid, Animation animation)
        {
            lock (SubscribedEntities)
            {
                if (!SubscribedEntities.ContainsKey(entityGuid))
                    SubscribedEntities[entityGuid] = new Dictionary<string, Animation>();

                if(!SubscribedEntities[entityGuid].ContainsKey(animation.Name))
                    SubscribedEntities[entityGuid].Add(animation.Name, animation);
            }
        }

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

        public bool IsPlaying(string entityGuid, string animationName)
        {
            lock(SubscribedEntities)
                return SubscribedEntities.ContainsKey(entityGuid) && SubscribedEntities[entityGuid].ContainsKey(animationName);
        }

        private Dictionary<String, Dictionary<string, Animation>> SubscribedEntities = new Dictionary<String, Dictionary<string, Animation>>();
        private TimeSpan LastTick;
    }
}
