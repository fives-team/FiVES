using System;

namespace NativeClient
{
    internal struct UpdateInfo
    {
        public string entityGuid;
        public string componentName;
        public string attributeName;
        public int timeStamp; /* not used yet */
        public object value;
    }
}

