using System;
using FIVES;
using System.Collections.Generic;
using KIARA;

namespace DirectCall
{
    /// <summary>
    /// DirectCall protocol factory implementation.
    /// </summary>
    /// <remarks>
    /// TODO: This should be rewritten such that the server is running on a different thread. For this we need to add
    /// synchronization primities to avoid race conditions.
    /// </remarks>
    public class DCProtocolFactory : IProtocolFactory
    {
        #region IProtocolFactory implementation

        public void openConnection(Server serverConfig, Context context, Action<IProtocol> onConnected)
        {
            Dictionary<string, Action<IProtocol>> serverList = getServerList(context);

            string id = validateServerConfigAndRetrieveId(serverConfig);
            if (!serverList.ContainsKey(id))
                throw new Error(ErrorCode.CONNECTION_ERROR, "Server with id '" + id + "' is not started.");

            // Create the protocol.
            var protocol = new DCProtocol();

            // Notify the server.
            serverList[id](protocol);

            // Notify the client.
            onConnected(protocol);
        }

        public void startServer(Server serverConfig, Context context, Action<IProtocol> onNewClient)
        {
            Dictionary<string, Action<IProtocol>> serverList = getServerList(context);

            string id = validateServerConfigAndRetrieveId(serverConfig);
            if (serverList.ContainsKey(id))
                throw new Error(ErrorCode.CONNECTION_ERROR, "Server with id '" + id + "' is already started.");

            serverList.Add(id, onNewClient);
        }

        private string validateServerConfigAndRetrieveId(Server serverConfig)
        {
            string protocol = ProtocolUtils.retrieveProtocolSetting<string>(serverConfig, "name", null);
            if (protocol != "direct-call")
                throw new Error(ErrorCode.CONNECTION_ERROR, "Given сonfig is not for direct-call protocol.");

            string id = ProtocolUtils.retrieveProtocolSetting<string>(serverConfig, "id", null);
            if (id == null)
                throw new Error(ErrorCode.CONNECTION_ERROR, "Config for direct-call must contain an id.");
            return id;
        }


        private Dictionary<string, Action<IProtocol>> getServerList(Context context)
        {
            if (!context.ProtocolData.ContainsKey("direct-call"))
                context.ProtocolData["direct-call"] = new Dictionary<string, Action<IProtocol>>();
            return (Dictionary<string, Action<IProtocol>>)context.ProtocolData["direct-call"];
        }
        #endregion
    }
}

