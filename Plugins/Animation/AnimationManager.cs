using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventLoopPlugin;

namespace AnimationPlugin
{
    internal class AnimationManager
    {
        public AnimationManager()
        {
            EventLoop.Instance.TickFired += new EventHandler<TickEventArgs>(handleEventTick);
        }

        private void handleEventTick(Object sender, TickEventArgs e)
        {
            Console.WriteLine("Handling EventTick for Animation at {0}", e.TimeStamp);
        }

        private Dictionary<Guid, Animation> subscribedEntities = new Dictionary<Guid, Animation>();
    }
}
