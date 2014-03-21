using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventLoopPlugin
{
    class EventLoopImpl : IEventLoop
    {
        public EventLoopImpl()
        {
            ReadIntervalFromConfig();
            stopwatch.Start();
            Task.Factory.StartNew(EventLoopThread, TaskCreationOptions.LongRunning);
        }

        public event EventHandler<TickEventArgs> TickFired;

        public int IntervalMs
        {
            get
            {
                return tickInterval;
            }
        }

        /// <summary>
        /// Reads the interval in which the events are fired from the App.cfg file
        /// </summary>
        private void ReadIntervalFromConfig()
        {
            string eventloopConfigPath = this.GetType().Assembly.Location;
            Configuration config = ConfigurationManager.OpenExeConfiguration(eventloopConfigPath);

            string configValue = config.AppSettings.Settings["tickinterval"].Value;
            int.TryParse(configValue, out tickInterval);
        }

        /// <summary>
        /// Function that fires the event periodically
        /// </summary>
        private void EventLoopThread()
        {
            while (true)
            {
                if (TickFired != null)
                    TickFired(this, new TickEventArgs(stopwatch.Elapsed));
                Thread.Sleep(tickInterval);
            }
        }

        private int tickInterval = 30;
        private Stopwatch stopwatch = new Stopwatch();
    }
}
