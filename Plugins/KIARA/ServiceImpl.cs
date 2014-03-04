using System;
using System.Collections.Generic;

namespace KIARAPlugin
{
    class ServiceImpl : Service, IServiceImpl
    {
        // FIXME: What do we do if we've had new clients before a handler is added? Should we keep the list of all
        // opened connections to invoke a new handler on each of them? What if some of these connection are closed
        // already?
        public event NewClient OnNewClient;

        internal void HandleNewClient(Connection connection)
        {
            RegisterMethods(connection);

            if (OnNewClient != null)
                OnNewClient(connection);
        }

        internal ServiceImpl(Context context) : base(context) {}
    }
}

