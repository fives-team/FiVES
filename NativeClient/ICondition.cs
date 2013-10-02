using System;

namespace NativeClient
{
    /// <summary>
    /// Condition for the state transition.
    /// </summary>
    public interface ICondition
    {
        /// <summary>
        /// Occurs when condition becomes satisfied.
        /// </summary>
        event EventHandler OnSatisfied;

        /// <summary>
        /// Gets a value indicating whether this condition <see cref="NativeClient.ICondition"/> is satisfied.
        /// </summary>
        /// <value><c>true</c> if satisfied; otherwise, <c>false</c>.</value>
        bool Satisfied { get; }

        /// <summary>
        /// Activates this condition. Called immediately after entering the starting state for the transition.
        /// </summary>
        void Activate();

        /// <summary>
        /// Deactivates this condition. Called shortly before after executing transition action and entering into a
        /// new state.
        /// </summary>
        void Deactivate();
    }
}
