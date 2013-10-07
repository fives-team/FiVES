using System;

namespace NativeClient
{
    class UnixTimestamp
    {
        public static long Now
        {
            get
            {
                TimeSpan span = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
                return (long)span.TotalSeconds;
            }
        }
    }
}

