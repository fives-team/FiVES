using System;
using FIVES;
using System.Collections.Generic;
using KIARA;

namespace Editing
{
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
                PluginManager.Instance.OnPluginLoaded += delegate(string pluginName) {
                    if (pluginName == "ClientSync")
                        registerEditingAPI();
                };
            }
        }

        public void createEntityAt(float x, float y, float z)
        {
            Entity e = new Entity();
            e["position"].setFloatAttribute("x", x);
            e["position"].setFloatAttribute("y", y);
            e["position"].setFloatAttribute("z", z);
            EntityRegistry.Instance.addEntity(e);
        }

        // Register editing APIs with ClientSync plugin.
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

        #endregion
    }
}

