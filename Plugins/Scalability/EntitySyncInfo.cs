using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScalabilityPlugin
{
    class EntitySyncInfo
    {
        public Guid Guid;
        public Dictionary<string, ComponentSyncInfo> Components = new Dictionary<string, ComponentSyncInfo>();
    }
}
