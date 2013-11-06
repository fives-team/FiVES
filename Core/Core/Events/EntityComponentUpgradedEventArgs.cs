using System;

namespace FIVES
{
    public class EntityComponentUpgradedEventArgs
    {
        public EntityComponentUpgradedEventArgs(Entity aEntity, string aComponentName)
        {
            entity = aEntity;
            componentName = aComponentName;
        }

        public Entity entity { get; private set; }
        public string componentName { get; private set; }
    }
}

