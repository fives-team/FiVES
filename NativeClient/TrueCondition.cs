using System;

namespace NativeClient
{
    public class TrueCondition : ICondition
    {
        #region ICondition implementation

        public event EventHandler OnSatisfied
        {
            add { value(this, new EventArgs()); }
            remove { }
        }

        public void Activate()
        {
        }

        public void Deactivate()
        {
        }

        public bool Satisfied
        {
            get
            {
                return true;
            }
        }

        #endregion


    }
}

