using System;

namespace NativeClient
{
    class Timestamps
    {
        /// <summary>
        /// Computes Unix timestamp for the current moment.
        /// </summary>
        /// <value>The unix timestamp.</value>
        public static long UnixTimestamp
        {
            get
            {
                TimeSpan span = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
                return (long)span.TotalSeconds;
            }
        }

        /// <summary>
        /// Returns a number of milliseconds that has passed since start of the day. This value will be relatively
        /// small and may be precisely stored in a float.
        /// </summary>
        /// <value>Number of milliseconds since start of the day.</value>
        public static float FloatMilliseconds
        {
            get
            {
                TimeSpan span = (DateTime.Now - DateTime.Today);
                return (float)span.TotalMilliseconds;
            }
        }
    }
}

