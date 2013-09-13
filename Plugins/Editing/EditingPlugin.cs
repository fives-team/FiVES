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
                RegisterEditingAPI();
            } else {
                PluginManager.Instance.OnPluginInitialized += delegate(Object sender, PluginLoadedEventArgs e) {
                    if (e.pluginName == "ClientSync")
                        RegisterEditingAPI();
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
        public void CreateEntityAt(float x, float y, float z)
        {
            Entity entity = new Entity();
            entity["position"]["x"] = x;
            entity["position"]["y"] = y;
            entity["position"]["z"] = z;
            EntityRegistry.Instance.AddEntity(entity);
        }

        /// <summary>
        /// Registers editing APIs with the ClientSync plugin.
        /// </summary>
        private void RegisterEditingAPI() {
            var clientSync = ServiceFactory.DiscoverByName("clientsync", ContextFactory.GetContext("inter-plugin"));
            clientSync.OnConnected += delegate(Connection connection) {
                connection["registerClientService"]("editing", new Dictionary<string, Delegate> {
                    {"createEntityAt", (Action<float, float, float>)CreateEntityAt}
                });
            };
        }
    }
}

