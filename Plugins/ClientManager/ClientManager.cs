// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using AuthPlugin;
using FIVES;
using KIARA;
using KIARAPlugin;
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

        public ClientManager()
        {
            clientService = KIARAServerManager.Instance.KiaraService;

            string clientManagerIDL = File.ReadAllText("clientManager.kiara");
            KIARAPlugin.KIARAServerManager.Instance.KiaraServer.AmendIDL(clientManagerIDL);

            RegisterClientService("kiara", false, new Dictionary<string, Delegate>());
            RegisterClientMethod("kiara.implements", false, (Func<List<string>, List<bool>>)Implements);
            RegisterClientMethod("kiara.implements", true, (Func<List<string>, List<bool>>)AuthenticatedImplements);

            RegisterClientService("auth", false, new Dictionary<string, Delegate> {
                {"login", (Func<Connection, string, string, bool>)Authenticate}
            });

            RegisterClientMethod("getTime", false, (Func<DateTime>)GetTime);

            RegisterClientService("objectsync", true, new Dictionary<string, Delegate> {
                {"listObjects", (Func<List<Dictionary<string, object>>>) ListObjects},
                {"notifyAboutNewObjects", (Action<Connection, Action<Dictionary<string, object>>>) NotifyAboutNewObjects},
                {"notifyAboutRemovedObjects", (Action<Connection, Action<string>>) NotifyAboutRemovedObjects},
                {"notifyAboutObjectUpdates",
                    (Action<Connection, Action<List<ClientUpdateQueue.UpdateInfo>>>) NotifyAboutObjectUpdates},
            });

            PluginManager.Instance.AddPluginLoadedHandler("Terminal", RegisterTerminalCommands);

            // DEBUG
            //            clientService["scripting.createServerScriptFor"] = (Action<string, string>)createServerScriptFor;
            //            clientService.OnNewClient += delegate(Connection connection) {
            //                var getAnswer = connection.generateFuncWrapper("getAnswer");
            //                getAnswer((Action<int>) delegate(int answer) { Console.WriteLine("The answer is {0}", answer); });
            //            };
        }

        private DateTime GetTime()
        {
            return DateTime.Now;
        }

        private void RegisterTerminalCommands()
        {
            Terminal.Instance.RegisterCommand("numClients", "Prints number of authenticated clients.", false,
                PrintNumClients, new List<string> { "nc" });
        }

        private void PrintNumClients(string commandLine)
        {
            Terminal.Instance.WriteLine("Number of connected clients: " + authenticatedClients.Count);
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
                    componentInfo[attrDefinition.Name] = component[attrDefinition.Name].Value;
                entityInfo[component.Name] = componentInfo;
            }

            return entityInfo;
        }

        bool Authenticate(Connection connection, string login, string password)
        {
            if (!Authentication.Instance.Authenticate(connection, login, password))
                return false;

            authenticatedClients.Add(connection);
            connection.Closed += HandleAuthenticatedClientDisconnected;

            if (OnAuthenticated != null)
                OnAuthenticated(connection);

            foreach (var entry in authenticatedMethods)
                connection.RegisterFuncImplementation(entry.Key, entry.Value);

            return true;
        }

        private void HandleAuthenticatedClientDisconnected(object sender, EventArgs e)
        {
            Connection connection = sender as Connection;

            if (onNewEntityHandlers.ContainsKey(connection))
            {
                foreach (var handler in onNewEntityHandlers[connection])
                    World.Instance.AddedEntity -= handler;
            }

            if (onRemovedEntityHandlers.ContainsKey(connection))
            {
                foreach (var handler in onRemovedEntityHandlers[connection])
                    World.Instance.RemovedEntity -= handler;
            }

            UpdateQueue.StopClientUpdates(connection);

            authenticatedClients.Remove(connection);
        }

        void NotifyAboutNewObjects(Connection connection, Action<Dictionary<string, object>> callback)
        {
            var handler = new EventHandler<EntityEventArgs>((sender, e) => callback(ConstructEntityInfo(e.Entity)));
            if (!onNewEntityHandlers.ContainsKey(connection))
                onNewEntityHandlers[connection] = new List<EventHandler<EntityEventArgs>>();
            onNewEntityHandlers[connection].Add(handler);
            World.Instance.AddedEntity += handler;
        }

        void NotifyAboutRemovedObjects(Connection connection, Action<string> callback)
        {
            var handler = new EventHandler<EntityEventArgs>((sender, e) => callback(e.Entity.Guid.ToString()));
            if (!onRemovedEntityHandlers.ContainsKey(connection))
                onRemovedEntityHandlers[connection] = new List<EventHandler<EntityEventArgs>>();
            onRemovedEntityHandlers[connection].Add(handler);
            World.Instance.RemovedEntity += handler;
        }

        void NotifyAboutObjectUpdates(Connection connection, Action<List<ClientUpdateQueue.UpdateInfo>> callback)
        {
            UpdateQueue.RegisterToClientUpdates(connection, callback);
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
        Dictionary<Connection, List<EventHandler<EntityEventArgs>>> onNewEntityHandlers =
            new Dictionary<Connection, List<EventHandler<EntityEventArgs>>>();
        Dictionary<Connection, List<EventHandler<EntityEventArgs>>> onRemovedEntityHandlers =
            new Dictionary<Connection, List<EventHandler<EntityEventArgs>>>();

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
