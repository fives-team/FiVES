using System;
using System.Configuration;
using KIARA;
using System.Collections.Generic;
using FIVES;

namespace ClientSync {

    public class ClientSyncPlugin : IPluginInitializer
    {
        public ClientSyncPlugin()
        {
            var context = new Context();
            string service = "http://localhost/projects/test-client/kiara/fives.json";
            context.acceptClients(service, registerMethods);
        }

        // This plugin uses KIARA to communicate with the clients. The following services are supported:
        //
        // service kiara {
        //   bool[] implements(string[] services);
        // }
        //
        // service clientsync {
        //   string[] listObjects();
        //   TODO: getObjectPosition
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
            List<Guid> guids = EntityRegistry.getAllGUIDs();
            return guids.ConvertAll(guid => guid.ToString());
        }

        private static void registerMethods(Connection connection)
        {
            connection.registerFuncImplementation("kiara.implements", "...",
                                                  (Func<List<string>, List<bool>>)implements);
            connection.registerFuncImplementation("clientsync.listObjects", "...", (Func<List<string>>)listObjects);
        }
    }

}