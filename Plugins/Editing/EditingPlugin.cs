using System;
using FIVES;
using System.Collections.Generic;
using KIARA;
using Events;

namespace Editing
{
    /// <summary>
    /// Plugin that allows changing the world by the users.
    /// </summary>
    public class EditingPlugin : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string GetName()
        {
            return "Editing";
        }

        public List<string> GetDependencies()
        {
            return new List<string>() { "Location" };
        }

        public void Initialize()
        {
            if (PluginManager.Instance.IsPluginLoaded("ClientSync")) {
                registerEditingAPI();
            } else {
                PluginManager.Instance.OnPluginInitialized += delegate(Object sender, PluginLoadedEventArgs e) {
                    if (e.pluginName == "ClientSync")
                        registerEditingAPI();
                };
            }
        }

        #endregion

        /// <summary>
        /// Creates an entity at x, y and z.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        public void createEntityAt(float x, float y, float z)
        {
            dynamic e = new Entity();
            e.position.x = x;
            e.position.y = y;
            e.position.z = z;
            EntityRegistry.Instance.AddEntity(e);
        }

        /// <summary>
        /// Registers editing APIs with the ClientSync plugin.
        /// </summary>
        private void registerEditingAPI() {
            var clientSync = ServiceFactory.discoverByName("clientsync", ContextFactory.getContext("inter-plugin"));
            clientSync.OnConnected += delegate(Connection connection) {
                var registerClientMethod = clientSync["registerClientMethod"];
                registerClientMethod("editing.createEntityAt", (Action<float, float, float>)createEntityAt);
            };
        }
    }
}

