using System;
using FIVES;
using System.Collections.Generic;
using KIARA;

namespace DirectCall
{
    public class DCProtocolFactory : IProtocolFactory
    {
        #region IProtocolFactory implementation

        public void openConnection(Server serverConfig, Action<IProtocol> onConnected)
        {
            string id = ProtocolUtils.retrieveProtocolSetting<string>(serverConfig, "id", null);
            if (id == null)
                throw new Error(ErrorCode.CONNECTION_ERROR, "Config for direct-call must contain an id.");

            if (!startedServers.ContainsKey(id))
                throw new Error(ErrorCode.CONNECTION_ERROR, "Server with id '" + id + "' is not started.");

            // Create the protocol.
            var protocol = new DCProtocol();

            // Notify the server.
            // TODO: Should we start a new thread?
            startedServers[id](protocol);

            // Notify the client.
            onConnected(protocol);
        }

        public void startServer(Server serverConfig, Action<IProtocol> onNewClient)
        {
            string id = ProtocolUtils.retrieveProtocolSetting<string>(serverConfig, "id", null);
            if (id == null)
                throw new Error(ErrorCode.CONNECTION_ERROR, "Config for direct-call must contain an id.");

            if (startedServers.ContainsKey(id))
                throw new Error(ErrorCode.CONNECTION_ERROR, "Server with id '" + id + "' is already started.");

            startedServers.Add(id, onNewClient);
        }

        #endregion

        private Dictionary<string, Action<IProtocol>> startedServers = new Dictionary<string, Action<IProtocol>>();
    }
}

