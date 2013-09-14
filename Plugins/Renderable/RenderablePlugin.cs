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
            registerComponent ();
        }

        private void registerComponent()
        {
            ComponentLayout rendereableComponentLayout = new ComponentLayout ();
            rendereableComponentLayout.AddAttribute<string>("uri");

            ComponentLayout scaleLayout = new ComponentLayout ();
            scaleLayout.AddAttribute<float>("x");
            scaleLayout.AddAttribute<float>("y");
            scaleLayout.AddAttribute<float>("z");

            ComponentRegistry.Instance.DefineComponent ("meshResource", this.pluginGUID, rendereableComponentLayout);
            ComponentRegistry.Instance.DefineComponent ("scale", this.pluginGUID, scaleLayout);
        }

        #endregion
        private readonly Guid pluginGUID = new Guid("aff8cd50-3214-41cb-b13e-583bc7b7ef3a");
    }
}

