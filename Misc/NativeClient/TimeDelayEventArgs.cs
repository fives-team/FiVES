using System;

namespace NativeClient
{
    public class TimeDelayEventArgs : EventArgs
    {
        public TimeDelayEventArgs(double timeDelayMs)
        {
            TimeDelayMs = timeDelayMs;
        }

        public double TimeDelayMs { get; private set; }
    }
}

