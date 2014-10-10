using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;

namespace LightPlugin
{
    public class LightPluginInitializer : IPluginInitializer
    {
        #region Plugin Initializer Interface

        public string Name
        {
            get { return "Light"; }
        }

        public List<string> PluginDependencies
        {
            get { return new List<string>(); }
        }

        public List<string> ComponentDependencies
        {
            get { return new List<string>(); }
        }

        public void Initialize()
        {
            registerLightComponent();
        }

        public void Shutdown()
        {
        }

        #endregion

        private void registerLightComponent()
        {
            ComponentDefinition lightComponent = new ComponentDefinition("light");
            lightComponent.AddAttribute<LightType>("type");
            lightComponent.AddAttribute<Vector>("intensity");
            lightComponent.AddAttribute<Vector>("attenuation");
            ComponentRegistry.Instance.Register(lightComponent);
        }
    }
}
