using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScalabilityPlugin
{
    internal struct AttributeSyncInfo
    {
        public long LastTimestamp;
        public object LastValue;
    }
}
