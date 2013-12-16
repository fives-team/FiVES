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
            
        }

        private Dictionary<Guid, Animation> subscribedEntities = new Dictionary<Guid, Animation>();
    }
}
