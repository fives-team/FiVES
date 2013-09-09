using System;
using System.Collections.Generic;

namespace KIARA
{
    public class ServiceImpl : Service
    {
        // FIXME: What do we do if we've had new clients before a handler is added? Should we keep the list of all 
        // opened connections to invoke a new handler on each of them? What if some of these connection are closed 
        // already?
        public delegate void NewClient(Connection connection);
        public event NewClient OnNewClient;

        public Delegate this[string name]
        {
            set
            {
                registeredMethods[name] = value;
            }
        }

        internal void HandleNewClient(Connection connection)
        {
            foreach (var entry in registeredMethods)
                connection.registerFuncImplementation(entry.Key, entry.Value);

            if (OnNewClient != null)
                OnNewClient(connection);
        }

        internal ServiceImpl(Context context) : base(context) {}

        private Dictionary<string, Delegate> registeredMethods = new Dictionary<string, Delegate>();
    }
}

