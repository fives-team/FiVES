using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientManagerPlugin
{
    public struct UpdateInfo
    {
        public string entityGuid;
        public string componentName;
        public string attributeName;
        //public int timeStamp; /* not used yet */
        public object value;
    }
}
