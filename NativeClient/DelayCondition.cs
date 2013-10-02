using System;
using System.Timers;

namespace NativeClient
{
    public class DelayCondition : ICondition
    {
        public DelayCondition(double delayInMilliseconds)
        {
            Timer = new Timer(delayInMilliseconds);
            Timer.Elapsed += HandleTimeElapsed;
        }

        void HandleTimeElapsed(object sender, ElapsedEventArgs e)
        {
            Satisfied = true;
            if (OnSatisfied != null)
                OnSatisfied(this, new EventArgs());
        }

        #region ICondition implementation

        public event EventHandler OnSatisfied;

        public void Activate()
        {
            Satisfied = false;
            Timer.Start();
        }

        public void Deactivate()
        {
            Timer.Stop();
        }

        public bool Satisfied { get; private set; }

        #endregion

        Timer Timer;
    }
}

