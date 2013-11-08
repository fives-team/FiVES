using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
{
    /// <summary>
    /// Event arguments for EntityAdded and EntityRemoved events in World class.
    /// </summary>
    public class EntityEventArgs : EventArgs
    {
        public EntityEventArgs(Entity entity)
        {
            Entity = entity;
        }

        public Entity Entity { get; private set; }
    }
}
