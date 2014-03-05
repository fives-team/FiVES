using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;

namespace FIVESServiceBus
{
    public class AttributeChangeEventArgs : EventArgs
    {

        public AttributeChangeEventArgs(Entity entity, string componentName, string attributeName, object value)
        {
            this.entity = entity;
            this.componentName = componentName;
            this.attributeName = attributeName;
            this.value = value;
        }

        public Entity Entity
        {
            get { return entity;  }
        }

        public string ComponentName
        {
            get { return componentName; }
        }

        public string AttributeName
        {
            get { return attributeName; }
        }

        public object Value
        {
            get
            {
                return value;
            }
        }

        Entity entity;
        string componentName;
        string attributeName;
        object value;
    }
}
