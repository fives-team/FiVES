using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;

namespace FIVESServiceBus
{
    public class EntityAttributeTransformationEventArgs : EventArgs
    {
        public EntityAttributeTransformationEventArgs(Entity entity)
        {
            this.entity = entity;
        }

        public Entity Entity
        {
            get { return entity; }
            private set { entity = value; }
        }

        private Entity entity;
    }
}
