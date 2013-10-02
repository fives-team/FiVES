using System;

namespace NativeClient
{
    /// <summary>
    /// Action to be executed when entering the state.
    /// </summary>
    public interface IStateAction
    {
        /// <summary>
        /// Executes the action.
        /// </summary>
        void Execute();
    }
}

