using System;

namespace NativeClient
{
    public class ExternalValueCondition : ICondition
    {
        public ExternalValueCondition(string requiredValue)
        {
            RequiredValue = requiredValue;
        }

        public void TrySatisfy(string givenValue)
        {
            if (givenValue == RequiredValue) {
                Satisfied = true;

                if (OnSatisfied != null)
                    OnSatisfied(this, new EventArgs());
            }
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

        string RequiredValue;
    }
}

