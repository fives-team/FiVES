// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using FIVES;
using SINFONI;
using SINFONIPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TerminalPlugin;

namespace ClientManagerPlugin
{
    public class ClientManager
    {
        public static ClientManager Instance;

        public event EventHandler<ClientConnectionEventArgs> NewClientConnected;
        public event EventHandler<ClientConnectionEventArgs> ClientDisconnected;

        public ClientManager()
        {
            InitializeSINFONI();
            RegisterClientServices();
            RegisterEventHandlers();
        }

        private void InitializeSINFONI()
        {
            clientService = SINFONIServerManager.Instance.SinfoniService;

            string clientManagerIDL = File.ReadAllText("clientManager.kiara");
            SINFONIPlugin.SINFONIServerManager.Instance.SinfoniServer.AmendIDL(clientManagerIDL);
        }

        private void RegisterClientServices()
        {
            RegisterClientService("kiara", false, new Dictionary<string, Delegate>());
            RegisterClientMethod("kiara.implements", false, (Func<List<string>, List<bool>>)Implements);
            RegisterClientMethod("kiara.implements", true, (Func<List<string>, List<bool>>)AuthenticatedImplements);

            RegisterClientMethod("getTime", false, (Func<DateTime>)GetTime);

            RegisterClientService("objectsync", true, new Dictionary<string, Delegate> {
                {"listObjects", (Func<List<Dictionary<string, object>>>) ListObjects},
                {"receiveNewObjects", (Action<Connection, Dictionary<string, object>>)HandleRemoteEntityAdded},
                {"removeObject", (Action<string>)HandleRemoteEntityRemoved},
                {"updateAttribute", (Action<string, string, string, object>)HandleAttributeUpdated}
            });
        }

        private void RegisterEventHandlers()
        {
            World.Instance.AddedEntity += new EventHandler<EntityEventArgs>(HandleEntityAdded);
            World.Instance.RemovedEntity += new EventHandler<EntityEventArgs>(HandleEntityRemoved);
            PluginManager.Instance.AddPluginLoadedHandler("Terminal", RegisterTerminalCommands);
        }

        private DateTime GetTime()
        {
            return DateTime.Now;
        }

        private void RegisterTerminalCommands()
        {
            Terminal.Instance.RegisterCommand("numClients", "Prints number of authenticated clients.", false,
                   PrintNumClients, new List<string> { "nc" });
            Terminal.Instance.RegisterCommand("funcImpl", "Prints function implementations provided by client manager", false,
                PrintRegisteredFunctions, new List<string>{"fi"});
        }

        private void PrintNumClients(string commandLine)
        {
            Terminal.Instance.WriteLine("Number of connected clients: " + authenticatedClients.Count);
        }

        private void PrintRegisteredFunctions(string commandLine)
        {
            Terminal.Instance.WriteLine("Authenticated Functions: ");
            foreach(string functionName in authenticatedMethods.Keys)
            {
                Terminal.Instance.WriteLine(functionName);
            }
        }

        #region Client interface

        Dictionary<string, object> ConstructEntityInfo(Entity entity)
        {
            var entityInfo = new Dictionary<string, object>();
            entityInfo["guid"] = entity.Guid;
            entityInfo["owner"] = entity.Owner;
            foreach (Component component in entity.Components)
            {
                var componentInfo = new Dictionary<string, object>();
                foreach (ReadOnlyAttributeDefinition attrDefinition in component.Definition.AttributeDefinitions)
                    componentInfo[attrDefinition.Name] = component[attrDefinition.Name].Value;
                entityInfo[component.Name] = componentInfo;
            }

            return entityInfo;
        }

        public bool ReceiveAuthenticatedClient(Connection connection)
        {

            authenticatedClients.Add(connection);
            connection.Closed += HandleAuthenticatedClientDisconnected;

            if (OnAuthenticated != null)
                OnAuthenticated(connection);

            foreach (var entry in authenticatedMethods)
                connection.RegisterFuncImplementation(entry.Key, entry.Value);

            WrapUpdateMethods(connection);
            if (NewClientConnected != null)
                NewClientConnected(this, new ClientConnectionEventArgs(connection));
            return true;
        }

        private void WrapUpdateMethods(Connection connection)
        {
            var newObjectUpdates = connection.GenerateClientFunction("objectsync", "receiveNewObjects");
            onNewEntityHandlers[connection] = newObjectUpdates;

            var removedObjectUpdates = connection.GenerateClientFunction("objectsync", "removeObject");
            onRemovedEntityHandlers[connection] = removedObjectUpdates;

            var updatedObjectUpdates = connection.GenerateClientFunction("objectsync", "receiveObjectUpdates");
            UpdateQueue.RegisterToClientUpdates(connection, updatedObjectUpdates);
        }

        private void HandleAuthenticatedClientDisconnected(object sender, EventArgs e)
        {
            Connection connection = sender as Connection;
            onNewEntityHandlers.Remove(connection);
            onRemovedEntityHandlers.Remove(connection);
            UpdateQueue.StopClientUpdates(connection);
            authenticatedClients.Remove(connection);
            if (ClientDisconnected != null)
                ClientDisconnected(this, new ClientConnectionEventArgs(connection));
        }

        private void HandleRemoteEntityAdded(Connection connection, Dictionary<string, object> EntityInfo)
        {
            if (EntityInfo["guid"] == null
                || EntityInfo["owner"] == null
                || EntityInfo["owner"].Equals(World.Instance.ID.ToString())
                || World.Instance.ContainsEntity(new Guid((string)EntityInfo["guid"]))
                )
                // Ignore added entity if it is either not correctly assigned to an owner, or when it was created by
                // the same instance, or when the entity was already added in a previous update
                return;

            Entity receivedEntity
                = new Entity(new Guid((string)EntityInfo["guid"]), connection.SessionID);

            foreach (KeyValuePair<string, object> entityComponent in EntityInfo)
            {
                string key = entityComponent.Key;
                if (key != "guid" && key != "owner")
                    ApplyComponent(receivedEntity, key, (Dictionary<string, object>)entityComponent.Value);
            }

            World.Instance.Add(receivedEntity);
        }

        private void HandleRemoteEntityRemoved(string entityGuid)
        {
            World.Instance.Remove(World.Instance.FindEntity(entityGuid));
        }

        private void HandleAttributeUpdated(string entityGuid, string componentName, string attributeName, object value)
        {
            var e = World.Instance.FindEntity(entityGuid);
            e[componentName][attributeName].Suggest(value);
        }

        private void ApplyComponent(Entity entity, string componentName, Dictionary<string, object> attributes)
        {
            foreach (KeyValuePair<string, object> attribute in attributes)
            {
                entity[componentName][attribute.Key].Suggest(attribute.Value);
            }
        }

        private void HandleEntityAdded(object sender, EntityEventArgs e)
        {
            foreach (KeyValuePair<Connection, ClientFunction> clientHandler in onNewEntityHandlers)
            {
                if(e.Entity.Owner != clientHandler.Key.SessionID)
                    clientHandler.Value(ConstructEntityInfo(e.Entity));
            }
        }

        private void HandleEntityRemoved(object sender, EntityEventArgs e)
        {
            foreach (KeyValuePair<Connection, ClientFunction> clientHandler in onRemovedEntityHandlers)
            {
                if(e.Entity.Owner != clientHandler.Key.SessionID)
                    clientHandler.Value(e.Entity.Guid.ToString());
            }
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
        ServiceImplementation clientService;

        /// <summary>
        /// List of authenticated clients.
        /// </summary>
        HashSet<Connection> authenticatedClients = new HashSet<Connection>();

        /// <summary>
        /// Methods that required user to authenticate before they become available.
        /// </summary>
        Dictionary<string, Delegate> authenticatedMethods = new Dictionary<string, Delegate>();

        /// <summary>
        /// List of handlers that need to be removed when client disconnects.
        /// </summary>
        Dictionary<Connection, ClientFunction> onNewEntityHandlers = new Dictionary<Connection, ClientFunction>();
        Dictionary<Connection, ClientFunction> onRemovedEntityHandlers = new Dictionary<Connection, ClientFunction>();

        event Action<Connection> OnAuthenticated;

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

        /// <summary>
        /// Calls the provided callback when a new client is connected. The connection to the new client is passed as a
        /// parameter for the callback.
        /// </summary>
        /// <param name="callback">The callback to be called.</param>
        public void NotifyWhenAnyClientAuthenticated(Action<Connection> callback)
        {
            OnAuthenticated += callback;
        }

        private ClientUpdateQueue UpdateQueue = new ClientUpdateQueue();
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
