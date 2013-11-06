using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewCorePrototype
{
    public class CreatedComponentEventArgs : EventArgs
    {
        public string ComponentName { get; private set; }
    }
}
