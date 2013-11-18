using System;
using FIVES;
using System.Collections.Generic;
using ClientManagerPlugin;

namespace RenderablePlugin
{
    public class RenderablePluginInitializer : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string GetName()
        {
            return "Renderable";
        }

        public List<string> GetDependencies()
        {
            return new List<string>() { "Location" };
        }

        public void Initialize()
        {
            DefineComponents();

            PluginManager.Instance.AddPluginLoadedHandler("ClientManager", (Action)RegisterClientServices);
        }

        private void DefineComponents()
        {
            ComponentDefinition renderable = new ComponentDefinition("meshResource");
            renderable.AddAttribute<string>("uri");
            renderable.AddAttribute<bool>("visible", true);
            ComponentRegistry.Instance.Register(renderable);

            ComponentDefinition scale = new ComponentDefinition("scale");
            scale.AddAttribute<float>("x", 1f);
            scale.AddAttribute<float>("y", 1f);
            scale.AddAttribute<float>("z", 1f);
            ComponentRegistry.Instance.Register(scale);
        }

        void RegisterClientServices()
        {
            ClientManager.Instance.RegisterClientService("mesh", true, new Dictionary<string, Delegate> {
                {"notifyAboutVisibilityUpdates", (Action<string, Action<bool>>) NotifyAboutVisibilityUpdates},
            });
        }

        private void NotifyAboutVisibilityUpdates(string guid, Action<bool> callback)
        {
            var entity = World.Instance.FindEntity(guid);
            entity["meshResource"].ChangedAttribute += (sender, e) =>
            {
                if (e.AttributeName == "visible")
                    callback((bool)e.NewValue);
            };
        }

        #endregion
    }
}

