using System;
using System.Collections.Generic;

namespace KIARAPlugin
{
    abstract class Service : IService
    {
        /// <summary>
        /// Returns the associated context for this service.
        /// </summary>
        /// <value>The associated context.</value>
        public Context Context { get; private set; }

        public Delegate this[string name]
        {
            set
            {
                registeredMethods[name] = value;
            }
        }

        protected Service(Context aContext)
        {
            Context = aContext;
        }

        protected void RegisterMethods(Connection connection)
        {
            foreach (var entry in registeredMethods)
                connection.RegisterFuncImplementation(entry.Key, entry.Value);
        }

        Dictionary<string, Delegate> registeredMethods = new Dictionary<string, Delegate>();
    }
}

