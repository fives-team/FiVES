using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptingPlugin
{
    /// <summary>
    /// Event arguments used for the NewContextCreated event in the IScripting interface.
    /// </summary>
    public class NewContextCreatedArgs : EventArgs
    {
        public NewContextCreatedArgs(Entity entity, IJSContext context)
        {
            Entity = entity;
            Context = context;
        }

        /// <summary>
        /// The entity for which the context was created for.
        /// </summary>
        public Entity Entity { get; private set; }

        /// <summary>
        /// The created context.
        /// </summary>
        public IJSContext Context { get; private set; }
    }
}
