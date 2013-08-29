using System;

namespace FIVES
{
    public class ComponentUpgradedEventArgs
    {
        public ComponentUpgradedEventArgs(string aComponentName)
        {
            componentName = aComponentName;
        }

        public string componentName { get; private set; }
    }
}

