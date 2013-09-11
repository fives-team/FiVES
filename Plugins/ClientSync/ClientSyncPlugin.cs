using System;
using KIARA;
using System.Collections.Generic;
using FIVES;

namespace ClientSync {

    public struct Vector {
        public float x, y, z;
    }

    public struct Quat {
        public float x, y, z, w;
    }

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
            return new List<string>() { "WebSocketJSON", "DirectCall", "Location", "Renderable", "Auth" };
        }

        public void Initialize()
        {
            clientService = ServiceFactory.CreateByURI("http://localhost/projects/test-client/kiara/fives.json");
            clientService["kiara.implements"] = (Func<List<string>, List<bool>>)Implements;
            clientService["clientsync.listObjects"] = (Func<List<EntityInfo>>)ListObjects;
            clientService["clientsync.setEntityLocation"] = (Action<string, Vector, Quat>)SetEntityLocation;
            clientService["clientsync.notifyAboutNewObjects"] = (Action<FuncWrapper>)NotifyAboutNewObjects;
            clientService["clientsync.notifyAboutRemovedObjects"] = (Action<FuncWrapper>)NotifyAboutRemovedObjects;
            clientService["clientsync.notifyAboutEntityLocationUpdates"] =
                (Action<string, Action<Vector, Quat>>)NotifyAboutEntityLocationUpdates;


            // DEBUG
//            clientService["scripting.createServerScriptFor"] = (Action<string, string>)createServerScriptFor;
//            clientService.OnNewClient += delegate(Connection connection) {
//                var getAnswer = connection.generateFuncWrapper("getAnswer");
//                getAnswer((Action<int>) delegate(int answer) { Console.WriteLine("The answer is {0}", answer); });
//            };

            var pluginService = ServiceFactory.CreateByName("clientsync", ContextFactory.GetContext("inter-plugin"));
            pluginService["registerClientMethod"] = (Action<string, Delegate>)RegisterClientMethod;
            pluginService["registerClientService"] = (Action<string,Dictionary<string, Delegate>>)RegisterClientService;
            pluginService["notifyWhenClientDisconnected"] = (Action<Guid,Action<Guid>>)NotifyWhenClientDisconnected;
        }

        #endregion

        #region Client interface

        private struct EntityInfo {
            public string guid;
            public string meshURI;
            public Vector position;
            public Quat orientation;
            public Vector scale;
        }

        private EntityInfo ConstuctEntityInfo(Guid elementId)
        {
            dynamic entity = EntityRegistry.Instance.GetEntity(elementId);

            var entityInfo = new EntityInfo();
            entityInfo.guid = entity.guid;
            entityInfo.position.x = entity.position.x;
            entityInfo.position.y = entity.position.y;
            entityInfo.position.z = entity.position.z;
            entityInfo.orientation.x = entity.orientation.x;
            entityInfo.orientation.y = entity.orientation.y;
            entityInfo.orientation.z = entity.orientation.z;
            entityInfo.orientation.w = entity.orientation.w;
            entityInfo.scale.x = entity.scale.x;
            entityInfo.scale.y = entity.scale.y;
            entityInfo.scale.z = entity.scale.z;
            entityInfo.meshURI = entity.meshResource.uri;
            return entityInfo;
        }

        void SetEntityLocation (string guid, Vector position, Quat orientation)
        {
            dynamic entity = EntityRegistry.Instance.GetEntity(guid) as dynamic;
            entity.position.x = position.x;
            entity.position.y = position.y;
            entity.position.z = position.z;
            entity.orientation.x = orientation.x;
            entity.orientation.y = orientation.y;
            entity.orientation.z = orientation.z;
            entity.orientation.w = orientation.w;
        }

        private void NotifyAboutNewObjects(FuncWrapper callback)
        {
            EntityRegistry.Instance.OnEntityAdded += (sender, e) => callback(ConstuctEntityInfo(e.elementId));
        }

        private void NotifyAboutRemovedObjects(FuncWrapper callback)
        {
            EntityRegistry.Instance.OnEntityRemoved += (sender, e) => callback(e.elementId.ToString());
        }

        void NotifyAboutEntityLocationUpdates (string guid, Action<Vector, Quat> callback)
        {
            var entity = EntityRegistry.Instance.GetEntity(guid);
            entity["position"].OnAttributeChanged += delegate(object sender, Events.AttributeChangedEventArgs ev) {
                dynamic e = entity as dynamic;
                callback(new Vector { x = e.position.x, y = e.position.y, z = e.position.z },
                         new Quat { x = e.orientation.x, y = e.orientation.y, z = e.orientation.z,
                                    w = e.orientation.w });
            };
        }

        private List<string> supportedServices = new List<string> { "kiara", "clientsync" };
        private List<bool> Implements(List<string> services)
        {
            return services.ConvertAll(supportedServices.Contains);
        }

        private List<EntityInfo> ListObjects()
        {
            var guids = EntityRegistry.Instance.GetAllGUIDs();
            List<EntityInfo> infos = new List<EntityInfo>();
            foreach (var guid in guids)
                infos.Add(ConstuctEntityInfo(guid));
            return infos;
        }

        private void CreateServerScriptFor(string guid, string script)
        {
            dynamic entity = EntityRegistry.Instance.GetEntity(guid);
            entity["scripting"]["serverScript"] = script;
        }

        private ServiceImpl clientService;

        #endregion

        #region Plugin interface

        private void RegisterClientService(string serviceName, Dictionary<string, Delegate> methods)
        {
            foreach (var method in methods)
                RegisterClientMethod(serviceName + "." + method.Key, method.Value);
            supportedServices.Add(serviceName);
        }

        private void RegisterClientMethod(string name, Delegate handler)
        {
            clientService[name] = handler;
        }

        private void NotifyWhenClientDisconnected(Guid secToken, Action<Guid> callback)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}