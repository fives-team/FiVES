using System;

namespace KIARAPlugin
{
    public delegate void Connected(Connection connection);

    public interface IServiceWrapper : IService
    {
        /// <summary>
        /// Occurs when connection is established. New handlers are also invoked, even if connection have been
        /// established before they were added.
        /// </summary>
        event Connected OnConnected;
    }
}

