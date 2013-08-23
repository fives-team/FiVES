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

        public string getName()
        {
            return "Editing";
        }

        public List<string> getDependencies()
        {
            return new List<string>() { "Location" };
        }

        public void initialize()
        {
            if (PluginManager.Instance.isPluginLoaded("ClientSync")) {
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
            e.position.setFloatAttribute("x", x);
            e.position.setFloatAttribute("y", y);
            e.position.setFloatAttribute("z", z);
            EntityRegistry.Instance.addEntity(e);
        }

        /// <summary>
        /// Registers editing APIs with the ClientSync plugin.
        /// </summary>
        private void registerEditingAPI() {
            var context = new Context();
            string pluginConfig = "data:text/json;base64,ewogICdpbmZvJzogJ0NsaWVudFN5bmNQbHVnaW4nLAogICdpZGxDb250" +
                "ZW50JzogJy4uLicsCiAgJ3NlcnZlcnMnOiBbewogICAgJ3NlcnZpY2VzJzogJyonLAogICAgJ3Byb3RvY29sJzogewogICAg" +
                    "ICAnbmFtZSc6ICdkaXJlY3QtY2FsbCcsCiAgICAgICdpZCc6ICdjbGllbnRzeW5jJywKICAgICB9LAogIH1dLAp9Cg==";
            context.openConnection(pluginConfig, delegate(Connection conn) {
                var registerClientMethod = conn.generateFuncWrapper("registerClientMethod");
                registerClientMethod("editing.createEntityAt", (Action<float, float, float>)createEntityAt);
            });
        }
    }
}

