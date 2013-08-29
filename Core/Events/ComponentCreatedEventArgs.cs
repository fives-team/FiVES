using System;

namespace Events
{
    public class ComponentCreatedEventArgs
    {
        public ComponentCreatedEventArgs (string newComponentName, Guid newComponentId)
        {
            this.newComponentName = newComponentName;
            this.newComponentId = newComponentId;
        }

        public string newComponentName;
        public Guid newComponentId;
    }
}

