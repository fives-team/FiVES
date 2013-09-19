using System;
using KIARA;
using System.Collections.Generic;
using FIVES;

namespace ClientManager {

    public struct Vector {
        public float x, y, z;
    }

    public struct Quat {
        public float x, y, z, w;
    }

    /// <summary>
    /// Implements a plugin that can be used to communicate with clients using KIARA.
    /// </summary>
    public class ClientManagerPlugin : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string GetName()
        {
            return "ClientManager";
        }

        public List<string> GetDependencies()
        {
            return new List<string>() { "WebSocketJSON", "DirectCall", "Location", "Renderable", "Auth" };
        }

        public void Initialize()
        {
            clientService = ServiceFactory.CreateByURI("http://localhost/projects/test-client/kiara/fives.json");
            var authService = ServiceFactory.DiscoverByName("auth", ContextFactory.GetContext("inter-plugin"));
            authService.OnConnected += (connection) => authPlugin = connection;

            RegisterClientService("kiara", new Dictionary<string, Delegate>(), false);
            RegisterClientMethod("kiara.implements", (Func<List<string>, List<bool>>)Implements, false);
            RegisterClientMethod("kiara.implements", (Func<List<string>, List<bool>>)AuthenticatedImplements);

            RegisterClientService("auth", new Dictionary<string, Delegate>(), false);
            clientService.OnNewClient += delegate(Connection connection) {
                connection.RegisterFuncImplementation("auth.login",
                    (Func<string, string, string>) delegate(string login, string password) {
                        Guid sessionKey = authPlugin["authenticate"](login, password).Wait<Guid>();
                        if (sessionKey == Guid.Empty)
                            return "";
                        authenticatedClients[sessionKey] = connection;
                        foreach (var entry in authenticatedMethods)
                            connection.RegisterFuncImplementation(entry.Key, entry.Value);
                        return sessionKey.ToString();
                    }
                );
            };

            RegisterClientService("objectsync", new Dictionary<string, Delegate> {
                {"listObjects", (Func<List<EntityInfo>>) ListObjects},
                {"setEntityLocation", (Action<string, Vector, Quat>) SetEntityLocation},
                {"notifyAboutNewObjects", (Action<Action<EntityInfo>>) NotifyAboutNewObjects},
                {"notifyAboutNewObjects", (Action<Action<string>>) NotifyAboutRemovedObjects},
                {
                    "notifyAboutEntityLocationUpdates",
                    (Action<string, Action<Vector, Quat>>) NotifyAboutEntityLocationUpdates
                },
            });

            // DEBUG
//            clientService["scripting.createServerScriptFor"] = (Action<string, string>)createServerScriptFor;
//            clientService.OnNewClient += delegate(Connection connection) {
//                var getAnswer = connection.generateFuncWrapper("getAnswer");
//                getAnswer((Action<int>) delegate(int answer) { Console.WriteLine("The answer is {0}", answer); });
//            };

            var pluginService = ServiceFactory.CreateByName("objectsync", ContextFactory.GetContext("inter-plugin"));
            pluginService["registerClientMethod"] = (Action<string, Delegate,bool>)RegisterClientMethod;
            pluginService["registerClientService"] =
                (Action<string,Dictionary<string, Delegate>,bool>)RegisterClientService;
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

        private void SetEntityLocation (string guid, Vector position, Quat orientation)
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

        private void NotifyAboutNewObjects(Action<EntityInfo> callback)
        {
            EntityRegistry.Instance.OnEntityAdded += (sender, e) => callback(ConstuctEntityInfo(e.elementId));
        }

        private void NotifyAboutRemovedObjects(Action<string> callback)
        {
            EntityRegistry.Instance.OnEntityRemoved += (sender, e) => callback(e.elementId.ToString());
        }

        private void NotifyAboutEntityLocationUpdates (string guid, Action<Vector, Quat> callback)
        {
            var entity = EntityRegistry.Instance.GetEntity(guid);
            entity["position"].OnAttributeChanged += delegate(object sender, Events.AttributeChangedEventArgs ev) {
                dynamic e = entity as dynamic;
                callback(new Vector { x = e.position.x, y = e.position.y, z = e.position.z },
                         new Quat { x = e.orientation.x, y = e.orientation.y, z = e.orientation.z,
                                    w = e.orientation.w });
            };
        }

        private List<string> basicClientServices = new List<string>();
        private List<bool> Implements(List<string> services)
        {
            return services.ConvertAll(basicClientServices.Contains);
        }

        private List<string> authenticatedClientServices = new List<string>();
        private List<bool> AuthenticatedImplements(List<string> services)
        {
            return services.ConvertAll(authenticatedClientServices.Contains);
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

        /// <summary>
        /// The client service.
        /// </summary>
        ServiceImpl clientService;

        /// <summary>
        /// Auth plugin service.
        /// </summary>
        Connection authPlugin;

        /// <summary>
        /// List of authenticated clients.
        /// </summary>
        Dictionary<Guid, Connection> authenticatedClients = new Dictionary<Guid, Connection>();

        /// <summary>
        /// Methods that required user to authenticate before they become available.
        /// </summary>
        Dictionary<string, Delegate> authenticatedMethods = new Dictionary<string, Delegate>();

        #endregion

        #region Plugin interface

        /// <summary>
        /// Registers the client service.
        /// </summary>
        /// <example>
        /// RegisterClientService("editing", new Dictionary<string, Delegate> {
        ///   {"createObject", (Func<Location, MeshData, string>)CreateObject},
        ///   {"deleteObject", (Action<string>)DeleteObject},
        /// };
        /// </example>
        /// <param name="serviceName">Service name.</param>
        /// <param name="methods">Methods (a map from the name to a delegate).</param>
        /// <param name="requireAuthentication">If set to <c>true</c> require clients to authenticate.</param>
        public void RegisterClientService(string serviceName, Dictionary<string, Delegate> methods,
                                           bool requireAuthentication = true)
        {
            foreach (var method in methods)
                RegisterClientMethod(serviceName + "." + method.Key, method.Value, requireAuthentication);
            if (!requireAuthentication)
                basicClientServices.Add(serviceName);
            authenticatedClientServices.Add(serviceName);
        }

        /// <summary>
        /// Registers the client method.
        /// </summary>
        /// <example>
        /// RegisterClientMethod("login", (Func<string,string,bool>)LoginToServer, false);
        /// </example>
        /// <param name="methodName">Method name.</param>
        /// <param name="handler">Delegate with implementation.</param>
        /// <param name="requireAuthentication">If set to <c>true</c> require clients to authenticate.</param>
        public void RegisterClientMethod(string methodName, Delegate handler, bool requireAuthentication = true)
        {
            if (requireAuthentication)
                authenticatedMethods[methodName] = handler;
            else
                clientService[methodName] = handler;
        }

        public void NotifyWhenClientDisconnected(Guid secToken, Action<Guid> callback)
        {
            if (!authenticatedClients.ContainsKey(secToken))
                throw new Exception("Client with with given session key {0} is not authenticated.");

            authenticatedClients[secToken].OnClose += (reason) => callback(secToken);
        }

        #endregion
    }

}