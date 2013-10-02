using System;
using System.Collections.Generic;

namespace NativeClient
{
    /// <summary>
    /// State transition for the client's state machine.
    /// </summary>
    public class StateTransition
    {
        public StateTransition(string startingState, string endingState, ICondition condition, int priority = 100)
        {
            StartingState = startingState;
            EndingState = endingState;
            Condition = condition;
            Priority = priority;
        }

        public string StartingState;
        public string EndingState;
        public ICondition Condition;
        public int Priority;
    }
}
