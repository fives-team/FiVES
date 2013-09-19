using System;
using FIVES;
using System.Collections.Generic;

namespace Renderable
{
    public class RenderablePlugin : IPluginInitializer
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
            RegisterComponent ();
        }

        private void RegisterComponent()
        {
            ComponentLayout rendereableComponentLayout = new ComponentLayout ();
            rendereableComponentLayout.AddAttribute<string>("uri");
            rendereableComponentLayout.AddAttribute<bool>("visible", true);

            ComponentLayout scaleLayout = new ComponentLayout ();
            scaleLayout.AddAttribute<float>("x", 1);
            scaleLayout.AddAttribute<float>("y", 1);
            scaleLayout.AddAttribute<float>("z", 1);

            ComponentRegistry.Instance.DefineComponent ("meshResource", this.pluginGUID, rendereableComponentLayout);
            ComponentRegistry.Instance.DefineComponent ("scale", this.pluginGUID, scaleLayout);
        }

        #endregion
        private readonly Guid pluginGUID = new Guid("aff8cd50-3214-41cb-b13e-583bc7b7ef3a");
    }
}

