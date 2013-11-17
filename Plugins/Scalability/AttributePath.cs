using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScalabilityPlugin
{
    /// <summary>
    /// Represents a path to an attribute in an entity. Used as a key in the EntitySyncInfo.Attribute property.
    /// </summary>
    class AttributePath
    {
        public AttributePath(string componentName, string attributeName)
        {
            ComponentName = componentName;
            AttributeName = attributeName;
        }

        public string ComponentName { get; private set; }
        public string AttributeName { get; private set; }
    }
}
