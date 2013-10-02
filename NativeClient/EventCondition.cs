using System;

namespace NativeClient
{
    public class EventCondition : ICondition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NativeClient.EventCondition"/> class.
        /// </summary>
        /// <example>
        /// new EventCondition(h => someObject.OnSomeEvent += h);
        /// </example>
        /// <param name="handler">Handler.</param>
        public EventCondition(Action<EventHandler> handler)
        {
            handler(HandleEvent);
        }

        void HandleEvent(object sender, EventArgs e)
        {
            Satisfied = true;

            if (OnSatisfied != null)
                OnSatisfied(this, new EventArgs());
        }

        #region ICondition implementation

        public event EventHandler OnSatisfied;

        public void Activate() {
            Satisfied = false;
        }

        public void Deactivate()
        {
        }

        public bool Satisfied { get; private set; }

        #endregion


    }
}

