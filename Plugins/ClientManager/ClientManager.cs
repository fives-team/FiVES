using AuthPlugin;
using FIVES;
using KIARAPlugin;
using Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientManagerPlugin
{
    public class ClientManager
    {
        public static ClientManager Instance = new ClientManager();

        public ClientManager()
        {
            clientService = ServiceFactory.CreateByURI("http://localhost/projects/test-client/kiara/fives.json");

            RegisterClientService("kiara", false, new Dictionary<string, Delegate>());
            RegisterClientMethod("kiara.implements", false, (Func<List<string>, List<bool>>)Implements);
            RegisterClientMethod("kiara.implements", true, (Func<List<string>, List<bool>>)AuthenticatedImplements);

            RegisterClientService("auth", false, new Dictionary<string, Delegate>());
            clientService.OnNewClient += delegate(Connection connection)
            {
                connection.RegisterFuncImplementation("auth.login",
                    (Func<string, string, string>)delegate(string login, string password)
                {
                    Guid sessionKey = Authentication.Instance.Authenticate(login, password);
                    if (sessionKey == Guid.Empty)
                        return "";
                    authenticatedClients[sessionKey] = connection;
                    if (OnAuthenticated != null)
                        OnAuthenticated(sessionKey);
                    foreach (var entry in authenticatedMethods)
                        connection.RegisterFuncImplementation(entry.Key, entry.Value);
                    return sessionKey.ToString();
                }
                );
            };

            RegisterClientService("objectsync", true, new Dictionary<string, Delegate> {
                {"listObjects", (Func<List<Dictionary<string, object>>>) ListObjects},
                {"notifyAboutNewObjects", (Action<string, Action<Dictionary<string, object>>>) NotifyAboutNewObjects},
                {"notifyAboutRemovedObjects", (Action<string, Action<string>>) NotifyAboutRemovedObjects},
                {"notifyAboutObjectUpdates", (Action<string, Action<List<ClientUpdateQueue.UpdateInfo>>>) NotifyAboutObjectUpdates},
            });

            // DEBUG
            //            clientService["scripting.createServerScriptFor"] = (Action<string, string>)createServerScriptFor;
            //            clientService.OnNewClient += delegate(Connection connection) {
            //                var getAnswer = connection.generateFuncWrapper("getAnswer");
            //                getAnswer((Action<int>) delegate(int answer) { Console.WriteLine("The answer is {0}", answer); });
            //            };
        }

        #region Client interface

        internal class EntityInfo
        {
            public string guid;
            public Dictionary<string, Dictionary<string, object>> components =
                new Dictionary<string,Dictionary<string,object>>();
        }

        Dictionary<string, object> ConstructEntityInfo(Guid elementId)
        {
            var entity = EntityRegistry.Instance.GetEntity(elementId);

            // TODO: Generalize
            var entityInfo = new Dictionary<string, object> {
                { "guid", entity.Guid.ToString() },
                { "position", new Dictionary<string, object> {
                    { "x", entity["position"]["x"] },
                    { "y", entity["position"]["y"] },
                    { "z", entity["position"]["z"] },
                }},
                { "orientation", new Dictionary<string, object> {
                    { "x", entity["orientation"]["x"] },
                    { "y", entity["orientation"]["y"] },
                    { "z", entity["orientation"]["z"] },
                    { "w", entity["orientation"]["w"] }
                }},
                { "scale", new Dictionary<string, object> {
                    { "x", entity["scale"]["x"] },
                    { "y", entity["scale"]["y"] },
                    { "z", entity["scale"]["z"] },
                }},
                { "meshResource", new Dictionary<string, object> {
                    { "meshURI", entity["meshResource"]["uri"] },
                    { "visible", entity["meshResource"]["visible"] },
                }},
            };

            return entityInfo;
        }

        void NotifyAboutNewObjects(string sessionKey, Action<Dictionary<string, object>> callback)
        {
            var handler = new EntityRegistry.EntityAdded((sender, e) => callback(ConstructEntityInfo(e.elementId)));
            var guid = Guid.Parse(sessionKey);
            if (!onNewEntityHandlers.ContainsKey(guid))
                onNewEntityHandlers[guid] = new List<EntityRegistry.EntityAdded>();
            onNewEntityHandlers[guid].Add(handler);
            EntityRegistry.Instance.OnEntityAdded += handler;
        }

        void NotifyAboutRemovedObjects(string sessionKey, Action<string> callback)
        {
            var handler = new EntityRegistry.EntityRemoved((sender, e) => callback(e.elementId.ToString()));
            var guid = Guid.Parse(sessionKey);
            if (!onRemovedEntityHandlers.ContainsKey(guid))
                onRemovedEntityHandlers[guid] = new List<EntityRegistry.EntityRemoved>();
            onRemovedEntityHandlers[guid].Add(handler);
            EntityRegistry.Instance.OnEntityRemoved += (sender, e) => callback(e.elementId.ToString());
        }

        void NotifyAboutObjectUpdates(string sessionKey, Action<List<ClientUpdateQueue.UpdateInfo>> callback)
        {
            ClientUpdateQueue queueForClient = new ClientUpdateQueue(sessionKey, callback);
            clientUpdateHandlers.Add(new Guid(sessionKey), queueForClient);
        }

        List<string> basicClientServices = new List<string>();
        List<bool> Implements(List<string> services)
        {
            return services.ConvertAll(basicClientServices.Contains);
        }

        List<string> authenticatedClientServices = new List<string>();
        List<bool> AuthenticatedImplements(List<string> services)
        {
            return services.ConvertAll(authenticatedClientServices.Contains);
        }

        List<Dictionary<string, object>> ListObjects()
        {
            var guids = EntityRegistry.Instance.GetAllGUIDs();
            List<Dictionary<string, object>> infos = new List<Dictionary<string, object>>();
            foreach (var guid in guids)
                infos.Add(ConstructEntityInfo(guid));
            return infos;
        }

        void CreateServerScriptFor(string guid, string script)
        {
            var entity = EntityRegistry.Instance.GetEntity(guid);
            entity["scripting"]["serverScript"] = script;
        }

        /// <summary>
        /// The client service.
        /// </summary>
        ServiceImpl clientService;

        /// <summary>
        /// List of authenticated clients.
        /// </summary>
        Dictionary<Guid, Connection> authenticatedClients = new Dictionary<Guid, Connection>();

        /// <summary>
        /// Methods that required user to authenticate before they become available.
        /// </summary>
        Dictionary<string, Delegate> authenticatedMethods = new Dictionary<string, Delegate>();

        /// <summary>
        /// List of handlers that need to be removed when client disconnects.
        /// </summary>
        Dictionary<Guid, List<EntityRegistry.EntityAdded>> onNewEntityHandlers =
            new Dictionary<Guid, List<EntityRegistry.EntityAdded>>();
        Dictionary<Guid, List<EntityRegistry.EntityRemoved>> onRemovedEntityHandlers =
            new Dictionary<Guid, List<EntityRegistry.EntityRemoved>>();
        Dictionary<Guid, ClientUpdateQueue> clientUpdateHandlers =
            new Dictionary<Guid, ClientUpdateQueue>();

        event Action<Guid> OnAuthenticated;

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
        public void RegisterClientService(string serviceName, bool requireAuthentication,
                                          Dictionary<string, Delegate> methods)
        {
            foreach (var method in methods)
                RegisterClientMethod(serviceName + "." + method.Key, requireAuthentication, method.Value);
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
        public void RegisterClientMethod(string methodName, bool requireAuthentication, Delegate handler)
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

            authenticatedClients[secToken].OnClose += delegate(string reason)
            {
                if (onNewEntityHandlers.ContainsKey(secToken))
                {
                    foreach (var handler in onNewEntityHandlers[secToken])
                        EntityRegistry.Instance.OnEntityAdded -= handler;
                }

                if (onRemovedEntityHandlers.ContainsKey(secToken))
                {
                    foreach (var handler in onRemovedEntityHandlers[secToken])
                        EntityRegistry.Instance.OnEntityRemoved -= handler;
                }

                if (clientUpdateHandlers.ContainsKey(secToken))
                {
                    clientUpdateHandlers[secToken].StopClientUpdates();
                    clientUpdateHandlers.Remove(secToken);
                }
                callback(secToken);
                authenticatedClients.Remove(secToken);
            };
        }

        public void NotifyWhenAnyClientAuthenticated(Action<Guid> callback)
        {
            OnAuthenticated += callback;
        }

        #endregion
    }
}
