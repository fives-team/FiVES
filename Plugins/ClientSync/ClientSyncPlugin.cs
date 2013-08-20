using System;
using KIARA;
using System.Collections.Generic;
using FIVES;

namespace ClientSync {

    /// <summary>
    /// Implements a plugin that can be used to communicate with clients using KIARA.
    /// </summary>
    public class ClientSyncPlugin : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string getName()
        {
            return "ClientSync";
        }

        public List<string> getDependencies()
        {
            return new List<string>() { "WebSocketJSON", "DirectCall" };
        }

        public void initialize()
        {
            clientContext.startServer("http://localhost/projects/test-client/kiara/fives.json", registerClientMethods);

            // {
            //   'info': 'ClientSyncPlugin',
            //   'idlContent': '...',
            //   'servers': [{
            //     'services': '*',
            //     'protocol': {
            //       'name': 'direct-call',
            //       'id': 'clientsync',
            //     },
            //   }],
            // }
            string pluginConfig = "data:text/json;base64,ewogICdpbmZvJzogJ0NsaWVudFN5bmNQbHVnaW4nLAogICdpZGxDb250ZW50" +
                "JzogJy4uLicsCiAgJ3NlcnZlcnMnOiBbewogICAgJ3NlcnZpY2VzJzogJyonLAogICAgJ3Byb3RvY29sJzogewogICAgICAnbmFt" +
                "ZSc6ICdkaXJlY3QtY2FsbCcsCiAgICAgICdpZCc6ICdjbGllbnRzeW5jJywKICAgICB9LAogIH1dLAp9Cg==";
            pluginContext.startServer(pluginConfig, registerPluginMethods);
        }

        #endregion

        private readonly List<string> supportedServices = new List<string> {
            "kiara",
            "clientsync"
        };

        private List<bool> implements(List<string> services)
        {
            return services.ConvertAll(service => supportedServices.Contains(service));
        }

        private List<string> listObjects()
        {
            HashSet<Guid> guids = EntityRegistry.Instance.getAllGUIDs();
            List<string> objects = new List<string>();
            foreach (var guid in guids)
                objects.Add(guid.ToString());
            return objects;
        }

        private class Position {
            public float x, y, z;
        }

        private Position getObjectPosition(string guid) {
            var entity = EntityRegistry.Instance.getEntity(new Guid(guid));
            var pos = new Position();
            pos.x = (float)entity["position"].getFloatAttribute("x");
            pos.y = (float)entity["position"].getFloatAttribute("y");
            pos.z = (float)entity["position"].getFloatAttribute("z");
            return pos;
        }

        private void registerClientMethods(Connection connection)
        {
            connection.registerFuncImplementation("kiara.implements", (Func<List<string>, List<bool>>)implements);
            connection.registerFuncImplementation("clientsync.listObjects", (Func<List<string>>)listObjects);
            connection.registerFuncImplementation("clientsync.getObjectPosition",
                                                  (Func<string, Position>)getObjectPosition);

            // Register custom client methods.
            foreach (var entry in clientMethods)
                connection.registerFuncImplementation(entry.Key, entry.Value);
        }

        private void registerClientMethod(string name, Delegate handler)
        {
            clientMethods.Add(name, handler);
        }

        private void registerPluginMethods(Connection connection)
        {
            connection.registerFuncImplementation("registerClientMethod",
                                                  (Action<string, Delegate>)registerClientMethod);
        }

        private Dictionary<string, Delegate> clientMethods = new Dictionary<string, Delegate>();

        private Context clientContext = new Context();
        private Context pluginContext = new Context();
    }

}