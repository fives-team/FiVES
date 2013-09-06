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
            clientService = ServiceFactory.createByURI("http://localhost/projects/test-client/kiara/fives.json");
            clientService["kiara.implements"] = (Func<List<string>, List<bool>>)implements;
            clientService["clientsync.listObjects"] = (Func<List<string>>)listObjects;
            clientService["clientsync.getObjectPosition"] = (Func<string, Position>)getObjectPosition;

            // DEBUG
            clientService["scripting.createServerScriptFor"] = (Action<string, string>)createServerScriptFor;

            var pluginService = ServiceFactory.createByName("clientsync", ContextFactory.getContext("inter-plugin"));
            pluginService["registerClientMethod"] = (Action<string, Delegate>)registerClientMethod;
        }

        #endregion

        private readonly List<string> supportedServices = new List<string> {
            "kiara",
            "clientsync"
        };

        private List<bool> implements(List<string> services)
        {
            return services.ConvertAll(supportedServices.Contains);
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
            dynamic entity = EntityRegistry.Instance.getEntity(new Guid(guid));
            var pos = new Position();
            pos.x = entity.position.x;
            pos.y = entity.position.y;
            pos.z = entity.position.z;
            return pos;
        }

        private void createServerScriptFor(string guid, string script)
        {
            dynamic entity = EntityRegistry.Instance.getEntity(guid);
            entity["scripting"]["serverScript"] = script;
        }

        private void registerClientMethod(string name, Delegate handler)
        {
            clientService[name] = handler;
        }

        private ServiceImpl clientService;
    }

}