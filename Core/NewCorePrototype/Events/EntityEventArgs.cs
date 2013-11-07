using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewCorePrototype
{
    /// <summary>
    /// Event arguments for EntityAdded and EntityRemoved event in World class.
    /// </summary>
    public class EntityEventArgs : EventArgs
    {
        internal EntityEventArgs(Entity entity)
        {
            Entity = entity;
        }

        public Entity Entity { get; private set; }
    }
}
