using System;

namespace KIARAPlugin
{
    public static class ServiceFactory
    {
        /// <summary>
        /// Creates a new service with description at <paramref name="configURI"/> using the default context.
        /// </summary>
        /// <returns>Created service.</returns>
        /// <param name="configURI">Configuration URI.</param>
        public static ServiceImpl Create(string configURI)
        {
            ServiceImpl service = new ServiceImpl(Context.DefaultContext);
            Context.DefaultContext.StartServer(configURI, service.HandleNewClient);
            return service;
        }

        public static ServiceWrapper Discover(string configURI)
        {
            ServiceWrapper service = new ServiceWrapper(Context.DefaultContext);
            Context.DefaultContext.OpenConnection(configURI, service.HandleConnected);
            return service;
        }
    }
}

