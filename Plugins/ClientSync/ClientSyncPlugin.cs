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

        public string GetName()
        {
            return "ClientSync";
        }

        public List<string> GetDependencies()
        {
            return new List<string>() { "WebSocketJSON", "DirectCall", "Location", "Renderable" };
        }

        public void Initialize()
        {
            clientService = ServiceFactory.createByURI("http://localhost/projects/test-client/kiara/fives.json");
            clientService["kiara.implements"] = (Func<List<string>, List<bool>>)implements;
            clientService["clientsync.listObjects"] = (Func<List<string>>)listObjects;
            clientService["clientsync.getObjectLocation"] = (Func<string, Location>)getObjectLocation;
            clientService["clientsync.getObjectMesh"] = (Func<string, Mesh>)getObjectMesh;
            clientService["clientsync.notifyAboutNewObjects"] = (Action<FuncWrapper>)notifyAboutNewObjects;
            clientService["clientsync.notifyAboutRemovedObjects"] = (Action<FuncWrapper>)notifyAboutRemovedObjects;

            // DEBUG
            clientService["scripting.createServerScriptFor"] = (Action<string, string>)createServerScriptFor;
//            clientService.OnNewClient += delegate(Connection connection) {
//                var getAnswer = connection.generateFuncWrapper("getAnswer");
//                getAnswer((Action<int>) delegate(int answer) { Console.WriteLine("The answer is {0}", answer); });
//            };

            var pluginService = ServiceFactory.createByName("clientsync", ContextFactory.getContext("inter-plugin"));
            pluginService["registerClientMethod"] = (Action<string, Delegate>)registerClientMethod;
        }

        #endregion

        private void notifyAboutNewObjects(FuncWrapper callback)
        {
            EntityRegistry.Instance.OnEntityAdded += (sender, e) => callback(e.elementId.ToString());
        }

        private void notifyAboutRemovedObjects(FuncWrapper callback)
        {
            EntityRegistry.Instance.OnEntityRemoved += (sender, e) => callback(e.elementId.ToString());
        }

        private void notifyAboutObjectUpdates(Action<string> callback)
        {
            throw new NotImplementedException();
        }

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
            HashSet<Guid> guids = EntityRegistry.Instance.GetAllGUIDs();
            List<string> objects = new List<string>();
            foreach (var guid in guids)
                objects.Add(guid.ToString());
            return objects;
        }

        private struct Vector {
            public float x, y, z;
        }

        private struct Quat {
            public float x, y, z, w;
        }

        private struct Location {
            public Vector position;
            public Quat orientation;
        }

        private Location getObjectLocation(string guid) {
            dynamic entity = EntityRegistry.Instance.GetEntity(new Guid(guid));
            var loc = new Location();
            loc.position.x = entity.position.x;
            loc.position.y = entity.position.y;
            loc.position.z = entity.position.z;
            loc.orientation.x = entity.orientation.x;
            loc.orientation.y = entity.orientation.y;
            loc.orientation.z = entity.orientation.z;
            loc.orientation.w = entity.orientation.w;
            return loc;
        }

        private struct Mesh {
            public string uri;
            public Vector scale;
        }

        private Mesh getObjectMesh(string guid) {
            dynamic entity = EntityRegistry.Instance.GetEntity(new Guid(guid));
            var mesh = new Mesh();
            mesh.uri = entity.meshResource.uri;
            mesh.scale.x = entity.scale.x;
            mesh.scale.y = entity.scale.y;
            mesh.scale.z = entity.scale.z;
            return mesh;
        }

        private void createServerScriptFor(string guid, string script)
        {
            dynamic entity = EntityRegistry.Instance.GetEntity(guid);
            entity["scripting"]["serverScript"] = script;
        }

        private void registerClientMethod(string name, Delegate handler)
        {
            clientService[name] = handler;
        }

        private ServiceImpl clientService;
    }

}