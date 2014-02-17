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
            ComponentDefinition mesh = new ComponentDefinition("mesh");
            mesh.AddAttribute<string>("uri");
            mesh.AddAttribute<bool>("visible", true);
            mesh.AddAttribute<Vector>("scale", new Vector(1, 1, 1));
            ComponentRegistry.Instance.Register(mesh);
        }

        #endregion
    }
}

