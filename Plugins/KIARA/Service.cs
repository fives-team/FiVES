using System;
using System.Collections.Generic;

namespace KIARAPlugin
{
    public abstract class Service
    {
        /// <summary>
        /// Returns the associated context for this service.
        /// </summary>
        /// <value>The associated context.</value>
        public Context context { get; private set; }

        public Delegate this[string name]
        {
            set
            {
                registeredMethods[name] = value;
            }
        }

        protected Service(Context aContext)
        {
            context = aContext;
        }

        protected void RegisterMethods(Connection connection)
        {
            foreach (var entry in registeredMethods)
                connection.RegisterFuncImplementation(entry.Key, entry.Value);
        }

        Dictionary<string, Delegate> registeredMethods = new Dictionary<string, Delegate>();
    }
}

