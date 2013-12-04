using AuthPlugin;
using FIVES;
using KIARAPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ClientManagerPlugin
{
    public class ClientManager
    {
        public static ClientManager Instance = new ClientManager();

        public ClientManager()
        {
            clientService = ServiceFactory.Create(ConvertFileNameToURI("clientManagerServer.json"));

            RegisterClientService("kiara", false, new Dictionary<string, Delegate>());
            RegisterClientMethod("kiara.implements", false, (Func<List<string>, List<bool>>)Implements);
            RegisterClientMethod("kiara.implements", true, (Func<List<string>, List<bool>>)AuthenticatedImplements);

            RegisterClientService("auth", false, new Dictionary<string, Delegate> {
                {"login", (Func<Connection, string, string, string>)Authenticate}
            });

            RegisterClientService("objectsync", true, new Dictionary<string, Delegate> {
                {"listObjects", (Func<List<Dictionary<string, object>>>) ListObjects},
                {"notifyAboutNewObjects", (Action<string, Action<Dictionary<string, object>>>) NotifyAboutNewObjects},
                {"notifyAboutRemovedObjects", (Action<string, Action<string>>) NotifyAboutRemovedObjects},
                {"notifyAboutObjectUpdates",
                    (Action<string, Action<List<ClientUpdateQueue.UpdateInfo>>>) NotifyAboutObjectUpdates},
            });

            // DEBUG
            //            clientService["scripting.createServerScriptFor"] = (Action<string, string>)createServerScriptFor;
            //            clientService.OnNewClient += delegate(Connection connection) {
            //                var getAnswer = connection.generateFuncWrapper("getAnswer");
            //                getAnswer((Action<int>) delegate(int answer) { Console.WriteLine("The answer is {0}", answer); });
            //            };
        }

        #region Client interface

        Dictionary<string, object> ConstructEntityInfo(Entity entity)
        {
            var entityInfo = new Dictionary<string, object>();
            entityInfo["guid"] = entity.Guid;

            foreach (Component component in entity.Components)
            {
                var componentInfo = new Dictionary<string, object>();
                foreach (ReadOnlyAttributeDefinition attrDefinition in component.Definition.AttributeDefinitions)
                    componentInfo[attrDefinition.Name] = component[attrDefinition.Name];
                entityInfo[component.Name] = componentInfo;
            }

            return entityInfo;
        }

        string Authenticate(Connection connection, string login, string password)
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

        void NotifyAboutNewObjects(string sessionKey, Action<Dictionary<string, object>> callback)
        {
            var handler = new EventHandler<EntityEventArgs>((sender, e) => callback(ConstructEntityInfo(e.Entity)));
            var guid = Guid.Parse(sessionKey);
            if (!onNewEntityHandlers.ContainsKey(guid))
                onNewEntityHandlers[guid] = new List<EventHandler<EntityEventArgs>>();
            onNewEntityHandlers[guid].Add(handler);
            World.Instance.AddedEntity += handler;
        }

        void NotifyAboutRemovedObjects(string sessionKey, Action<string> callback)
        {
            var handler = new EventHandler<EntityEventArgs>((sender, e) => callback(e.Entity.Guid.ToString()));
            var guid = Guid.Parse(sessionKey);
            if (!onRemovedEntityHandlers.ContainsKey(guid))
                onRemovedEntityHandlers[guid] = new List<EventHandler<EntityEventArgs>>();
            onRemovedEntityHandlers[guid].Add(handler);
            World.Instance.RemovedEntity += handler;
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
            List<Dictionary<string, object>> infos = new List<Dictionary<string, object>>();
            foreach (var entity in World.Instance)
                infos.Add(ConstructEntityInfo(entity));
            return infos;
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
        Dictionary<Guid, List<EventHandler<EntityEventArgs>>> onNewEntityHandlers =
            new Dictionary<Guid, List<EventHandler<EntityEventArgs>>>();
        Dictionary<Guid, List<EventHandler<EntityEventArgs>>> onRemovedEntityHandlers =
            new Dictionary<Guid, List<EventHandler<EntityEventArgs>>>();
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

            authenticatedClients[secToken].Closed += new EventHandler((sender, e) =>
            {
                if (onNewEntityHandlers.ContainsKey(secToken))
                {
                    foreach (var handler in onNewEntityHandlers[secToken])
                        World.Instance.AddedEntity -= handler;
                }

                if (onRemovedEntityHandlers.ContainsKey(secToken))
                {
                    foreach (var handler in onRemovedEntityHandlers[secToken])
                        World.Instance.RemovedEntity -= handler;
                }

                if (clientUpdateHandlers.ContainsKey(secToken))
                {
                    clientUpdateHandlers[secToken].StopClientUpdates();
                    clientUpdateHandlers.Remove(secToken);
                }
                callback(secToken);
                authenticatedClients.Remove(secToken);
            });
        }

        public void NotifyWhenAnyClientAuthenticated(Action<Guid> callback)
        {
            OnAuthenticated += callback;
        }

        public int GetNumAuthenticatedClients()
        {
            return authenticatedClients.Count;
        }

        #endregion

        /// <summary>
        /// Converts a file name to the URI that point to the file as if it was located in the same directory as the
        /// current assembly.
        /// </summary>
        /// <param name="configFilename"></param>
        /// <returns></returns>
        private string ConvertFileNameToURI(string configFilename)
        {
            var configFullPath = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), configFilename);
            return "file://" + configFullPath;
        }
    }
}
