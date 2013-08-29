using System;
using FIVES;
using System.Collections.Generic;

namespace Renderable
{
    public class RenderablePlugin : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string getName()
        {
            return "Renderable";
        }

        public List<string> getDependencies()
        {
            return new List<string>() {"Location"};
        }

        public void initialize()
        {
            registerComponent ();
        }

        private void registerComponent()
        {
            ComponentLayout rendereableComponentLayout = new ComponentLayout ();
            rendereableComponentLayout ["meshResource"] = typeof(string);
            ComponentRegistry.Instance.defineComponent ("Renderable", this.pluginGUID, rendereableComponentLayout);
        }

        #endregion
        private readonly Guid pluginGUID = new Guid("aff8cd50-3214-41cb-b13e-583bc7b7ef3a");
    }
}

