using System;

namespace NativeClient
{
    public class DelegateAction : IStateAction
    {
        public DelegateAction(Action action)
        {
            Action = action;
        }

        #region IStateAction implementation

        public void Execute()
        {
            Action();
        }

        #endregion

        Action Action;
    }
}

