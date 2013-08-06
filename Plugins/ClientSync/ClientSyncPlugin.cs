using System;
using KIARA;
using System.Collections.Generic;
using FIVES;

namespace ClientSync {

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

        // This plugin uses KIARA to communicate with the clients. The following services are supported:
        //
        // service kiara {
        //   bool[] implements(string[] services);
        // }
        //
        // struct Position {
        //   float x, y, z;
        // }
        //
        // service clientsync {
        //   string[] listObjects();
        //   Position getObjectPosition(string objID);
        // }

        private static readonly List<string> supportedServices = new List<string> {
            "kiara",
            "clientsync"
        };

        private static List<bool> implements(List<string> services)
        {
            return services.ConvertAll(service => supportedServices.Contains(service));
        }

        private static List<string> listObjects()
        {
            List<Guid> guids = EntityRegistry.Instance.getAllGUIDs();
            return guids.ConvertAll(guid => guid.ToString());
        }

        private class Position {
            public float x, y, z;
        }

        private static Position getObjectPosition(string guid) {
            var entity = EntityRegistry.Instance.getEntityByGuid(new Guid(guid));
            var pos = new Position();
            pos.x = (float)entity["position"].getFloatAttribute("x");
            pos.y = (float)entity["position"].getFloatAttribute("y");
            pos.z = (float)entity["position"].getFloatAttribute("z");
            return pos;
        }

        private static void registerClientMethods(Connection connection)
        {
            connection.registerFuncImplementation("kiara.implements", (Func<List<string>, List<bool>>)implements);
            connection.registerFuncImplementation("clientsync.listObjects", (Func<List<string>>)listObjects);
            connection.registerFuncImplementation("clientsync.getObjectPosition",
                                                  (Func<string, Position>)getObjectPosition);
        }

        private static void registerPluginMethods(Connection connection)
        {
            // TODO
        }

        private Context clientContext = new Context();
        private Context pluginContext = new Context();
    }

}