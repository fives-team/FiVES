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
            return new List<string>() {"Location"};
        }

        public void Initialize()
        {
            DefineComponents();

            PluginManager.Instance.AddPluginLoadedHandler("ClientManager", (Action)RegisterClientServices);
        }

        private void DefineComponents()
        {
            ComponentLayout rendereableComponentLayout = new ComponentLayout ();
            rendereableComponentLayout.AddAttribute<string>("uri");
            rendereableComponentLayout.AddAttribute<bool>("visible", true);

            ComponentLayout scaleLayout = new ComponentLayout ();
            scaleLayout.AddAttribute<float>("x", 1f);
            scaleLayout.AddAttribute<float>("y", 1f);
            scaleLayout.AddAttribute<float>("z", 1f);

            ComponentRegistry.Instance.DefineComponent ("meshResource", this.pluginGUID, rendereableComponentLayout);
            ComponentRegistry.Instance.DefineComponent ("scale", this.pluginGUID, scaleLayout);
        }

        void RegisterClientServices()
        {
            ClientManager.Instance.RegisterClientService("mesh", true, new Dictionary<string, Delegate> {
                {"notifyAboutVisibilityUpdates", (Action<string, Action<bool>>) NotifyAboutVisibilityUpdates},
            });
        }

        private void NotifyAboutVisibilityUpdates (string guid, Action<bool> callback)
        {
            var entity = EntityRegistry.Instance.GetEntity(guid);
            entity["meshResource"].OnAttributeChanged += delegate(object sender, Events.AttributeChangedEventArgs ev) {
                if (ev.AttributeName == "visible")
                    callback((bool)ev.NewValue);
            };
        }

        #endregion
        private readonly Guid pluginGUID = new Guid("aff8cd50-3214-41cb-b13e-583bc7b7ef3a");
    }
}

