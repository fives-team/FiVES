using System;
using WebSocket4Net;
using NLog;
using SuperSocket.ClientEngine;
using System.Collections.Generic;
using System.Threading;

namespace NativeClient
{
    public class StateMachine
    {
        public string CurrentState { get; private set; }

        public string FinalState { get; private set; }

        public StateMachine(string startingState) {
            CurrentState = startingState;
            FinalState = GenerateRandomStateName("final");
        }

        /// <summary>
        /// Adds an action to the state.
        /// </summary>
        /// <param name="stateName">State name.</param>
        /// <param name="action">Action.</param>
        public void AddStateAction(string stateName, IStateAction action)
        {
            EnsureState(stateName);

            StateActions[stateName].Add(action);
        }

        /// <summary>
        /// Adds a state transition.
        /// </summary>
        /// <param name="transition">Transition.</param>
        private void AddStateTransition(StateTransition transition)
        {
            EnsureState(transition.StartingState);
            EnsureState(transition.EndingState);

            Transitions[transition.StartingState].Add(transition);
        }

        public void AddStateTransition(string startingState, string endingState, ICondition condition)
        {
            AddStateTransition(new StateTransition(startingState, endingState, condition));
        }

        /// <summary>
        /// Creates an error state, which prints out given message and adds transitions from starting state to new error
        /// state and from error state to the final state.
        /// </summary>
        /// <param name="startingState">Starting state.</param>
        /// <param name="message">Message.</param>
        /// <param name="condition">Condition.</param>
        public void AddErrorTransition(string startingState, string message, ICondition condition)
        {
            string errorState = GenerateRandomStateName("error");
            AddStateAction(errorState, new DelegateAction(delegate {
                Logger.Error("Error in state " + PreviousState + ": " + message);
            }));
            AddStateTransition(startingState, errorState, condition);
            AddStateTransition(errorState, FinalState, new TrueCondition());
        }

        /// <summary>
        /// Creates an error state, which prints out give message and creates transitions from all currently registered
        /// states to this error state and from this state to the final state. This method should be used to add errors
        /// that can happen at any state, but must be called after all other states and transitions were added.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="condition">Condition.</param>
        public EventHandler GetUniversalErrorHandler(string message)
        {
            var errorState = GenerateRandomStateName("universal-error");
            AddStateAction(errorState, new DelegateAction(delegate {
                Logger.Error("Error in state " + PreviousState + ": " + message);
            }));

            ExternalCondition condition = new ExternalCondition();
            foreach (var state in States)
                AddStateTransition(new StateTransition(state, errorState, condition, 200));


            return new EventHandler((sender, e) => condition.Satisfy());
        }

        public void Run()
        {
            while (CurrentState != FinalState) {
                Logger.Debug("Entered state {0}", CurrentState);

                Logger.Debug("Executing actions for {0}", CurrentState);

                // Execute state action if any.
                if (StateActions[CurrentState] != null)
                    StateActions[CurrentState].ForEach(a => a.Execute());

                Logger.Debug("Activating conditions for {0}", CurrentState);

                // Activate all conditions on outgoing transitions.
                var possibleTransitions = Transitions[CurrentState];
                possibleTransitions.ForEach(p => p.Condition.Activate());

                // Sort transitions by prority. The passed function should return negative value if x should precede
                // y, and since we want larger priorities first, we reverse the results.
                possibleTransitions.Sort((x, y) => y.Priority - x.Priority);

                Logger.Debug("Checking conditions for {0}", CurrentState);

                // Search for sastisfied condition or wait until such appears.
                var activeTransition = possibleTransitions.Find(t => t.Condition.Satisfied);
                if (activeTransition == null) {
                    Logger.Debug("Waiting for satisfying condition for {0}", CurrentState);

                    AutoResetEvent conditionSatisfiedEvent = new AutoResetEvent(false);
                    possibleTransitions.ForEach(
                        t => t.Condition.OnSatisfied += (sender, e) => conditionSatisfiedEvent.Set());
                    conditionSatisfiedEvent.WaitOne();

                    activeTransition = possibleTransitions.Find(t => t.Condition.Satisfied);
                }

                Logger.Debug("Deactivating conditions for {0}", CurrentState);

                // Deactivate all conditions.
                possibleTransitions.ForEach(p => p.Condition.Deactivate());

                Logger.Debug("Switching state to {0}", activeTransition.EndingState);

                // Change state.
                PreviousState = CurrentState;
                CurrentState = activeTransition.EndingState;
            }
        }

        string GenerateRandomStateName(string prefix)
        {
            string stateName = null;
            do {
                Random randomizer = new Random();
                stateName = prefix + "-" + randomizer.Next();
            } while (States.Contains(stateName));

            return stateName;
        }

        void EnsureState(string state)
        {
            States.Add(state);

            if (!StateActions.ContainsKey(state))
                StateActions.Add(state, new List<IStateAction>());

            // Create collection of transitions from this state.
            if (!Transitions.ContainsKey(state))
                Transitions.Add(state, new List<StateTransition>());
        }

        // Set of transitions keyed by starting state.
        Dictionary<string, List<StateTransition>> Transitions = new Dictionary<string, List<StateTransition>>();

        // Actions associated with each state.
        Dictionary<string, List<IStateAction>> StateActions = new Dictionary<string, List<IStateAction>>();

        // Registered states. Used to create transitions to universal error state.
        HashSet<string> States = new HashSet<string>();

        // Stores previous state for error reporting.
        string PreviousState;

        static Logger Logger = LogManager.GetCurrentClassLogger();
    }
}

