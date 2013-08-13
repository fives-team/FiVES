using System;

namespace Events
{
    public class EntityAddedOrRemovedEventArgs : EventArgs
    {
        public EntityAddedOrRemovedEventArgs (Guid elementId)
        {
            this.elementId = elementId;
        }

        public Guid elementId { get; private set; }
    }
}

