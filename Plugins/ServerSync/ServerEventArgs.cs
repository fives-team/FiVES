using System;

namespace ServerSyncPlugin
{
    public class ServerEventArgs : EventArgs
    {
        public ServerEventArgs(IRemoteServer server)
        {
            Server = server;
        }

        public IRemoteServer Server { get; private set; }
    }
}
