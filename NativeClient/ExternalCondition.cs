using System;

namespace NativeClient
{
    public class ExternalCondition : ICondition
    {
        public void Satisfy()
        {
            Satisfied = true;

            if (OnSatisfied != null)
                OnSatisfied(this, new EventArgs());
        }

        #region ICondition implementation

        public event EventHandler OnSatisfied;

        public void Activate()
        {
        }

        public void Deactivate()
        {
        }

        public bool Satisfied { get; private set; }

        #endregion
    }
}

