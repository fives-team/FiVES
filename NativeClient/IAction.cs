using System;

namespace NativeClient
{
    /// <summary>
    /// Action for the state transition.
    /// </summary>
    public interface IAction
    {
        void Execute();
    }
}

