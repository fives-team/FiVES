using System;
using FIVES;
using System.Collections.Generic;

namespace ScriptingPlugin
{
    /// <summary>
    /// Scripting plugin.
    /// </summary>
    public class ScriptingPluginInitializer : IPluginInitializer
    {
        #region IPluginInitializer implementation

        /// <summary>
        /// Returns the name of the plugin.
        /// </summary>
        /// <returns>The name of the plugin.</returns>
        public string GetName()
        {
            return "Scripting";
        }

        /// <summary>
        /// Returns the list of names of the plugins that this plugin depends on.
        /// </summary>
        /// <returns>The list of names of the plugins that this plugin depends on.</returns>
        public List<string> GetDependencies()
        {
            return new List<string>();
        }

        /// <summary>
        /// Initializes the plugin. This method will be called by the plugin manager when all dependency plugins have
        /// been loaded.
        /// </summary>
        public void Initialize()
        {
            // Register 'scripting' component.
            ComponentLayout layout = new ComponentLayout();
            layout.AddAttribute<string>("ownerScript");
            layout.AddAttribute<string>("serverScript");
            layout.AddAttribute<string> ("clientScript");
            ComponentRegistry.Instance.DefineComponent("scripting", pluginGUID, layout);
        }

        #endregion

        private readonly Guid pluginGUID = new Guid("90dd4c50-f09d-11e2-b778-0800200c9a66");
    }
}

