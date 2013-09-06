using System;
using System.Collections.Generic;

namespace KIARA
{
    public class ServiceImpl : Service
    {
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

