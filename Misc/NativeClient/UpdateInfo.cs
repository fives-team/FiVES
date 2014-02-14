using System;

namespace NativeClient
{
    internal struct UpdateInfo
    {
#pragma warning disable 0649
        public string entityGuid;
        public string componentName;
        public string attributeName;
        //public int timeStamp; /* not used yet */
        public object value;
#pragma warning restore 0649
    }
}

