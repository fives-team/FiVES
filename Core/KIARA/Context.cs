using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;

namespace KIARA
{
    #region JSON Config structure
    public struct Server
    {
        public string services;
        public JToken protocol;
    }

    public struct Config
    {
        public string info;
        public string idlURL;
        public string idlContents;
        public List<Server> servers;
    }
    #endregion

    public class Context
    {
        // Opens a connection to a server specified in the config file retrived from |configURI|. Fragment part of the
        // |configURI| may be used to select the server by its index, e.g. http://www.example.org/config.json#3. If no
        // fragment is provided, or index is invalid, first server with supported protocol is chosen. Upon connection
        // |onConnected| is called with constructed Connection object.
        public void openConnection(string configURI, Action<Connection> onConnected)
        {
            Config config = retrieveConfig(configURI);
            Server server = selectServer(configURI, config);

            string protocolName = server.protocol["name"].ToString();
            IProtocolFactory protocolFactory = ProtocolRegistry.Instance.getProtocolFactory(protocolName);
            protocolFactory.openConnection(server, delegate(IProtocol p) {
                Connection conn = new Connection(p);
                onConnected(conn);
            });
        }

        // Creates a server specified in the config file retrieved from |configURI|. Fragment part of the |configURI|
        // may be used to select the server by its index, e.g. http://www.example.org/config.json#3. If no fragment is
        // provided, or index is invalid, first server with supported protocol is chosen. For each connected client
        // |onNewClient| is called with constructed Connection object. Note that |onNewClient| may be executed on a
        // different thread than the one you are calling from, depending on the implementation of the protocol specified
        // in the config file.
        public void startServer(string configURI, Action<Connection> onNewClient)
        {
            Config config = retrieveConfig(configURI);
            Server server = selectServer(configURI, config);

            string protocolName = server.protocol["name"].ToString();
            IProtocolFactory protocolFactory = ProtocolRegistry.Instance.getProtocolFactory(protocolName);
            protocolFactory.startServer(server, delegate(IProtocol p) {
                Connection conn = new Connection(p);
                onNewClient(conn);
            });
        }

        #region Private implementation
        private Config retrieveConfig(string configURI)
        {
            WebClient client = new WebClient();
            string config = client.DownloadString(configURI);
            return JsonConvert.DeserializeObject<Config>(config);
        }

        private bool isSupportedServerProtocol(Server server) {
            if (server.protocol == null)
                return false;

            JToken protocolName = server.protocol.SelectToken("name");
            if (protocolName == null)
                return false;

            return ProtocolRegistry.Instance.isRegistered(protocolName.ToString());
        }

        private Server selectServer(string configURI, Config config)
        {
            Uri parsedURI = new Uri(configURI);
            string fragment = parsedURI.Fragment;

            if (config.servers == null)
                throw new Error(ErrorCode.INIT_ERROR, "Configuration file contains no servers.");

            int serverNum = -1;
            if (!Int32.TryParse(fragment, out serverNum) || serverNum < 0 || serverNum >= config.servers.Count ||
                !isSupportedServerProtocol(config.servers[serverNum])) {
                serverNum = config.servers.FindIndex(s => isSupportedServerProtocol(s));
            }

            if (serverNum == -1)
                throw new Error(ErrorCode.INIT_ERROR, "Found no server with compatible protocol.");

            return config.servers[serverNum];
        }

        #endregion
    }
}
