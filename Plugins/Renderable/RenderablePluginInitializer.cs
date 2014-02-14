using System;
using FIVES;
using System.Collections.Generic;
using ClientManagerPlugin;

namespace RenderablePlugin
{
    public class RenderablePluginInitializer : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string Name
        {
            get
            {
                return "Renderable";
            }
        }

        public List<string> PluginDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        public List<string> ComponentDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        public void Initialize()
        {
            DefineComponents();
        }

        public void Shutdown()
        {
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

        #endregion
    }
}

