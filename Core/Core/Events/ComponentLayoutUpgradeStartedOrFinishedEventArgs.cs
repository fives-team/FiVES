using System;

namespace FIVES
{
    public class ComponentLayoutUpgradeStartedOrFinishedEventArgs
    {
        public ComponentLayoutUpgradeStartedOrFinishedEventArgs(string aComponentName)
        {
            componentName = aComponentName;
        }

        public string componentName { get; private set; }
    }
}

