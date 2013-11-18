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
            ComponentDefinition scripting = new ComponentDefinition("scripting");
            scripting.AddAttribute<string>("ownerScript");
            scripting.AddAttribute<string>("serverScript");
            scripting.AddAttribute<string> ("clientScript");
            ComponentRegistry.Instance.Register(scripting);
        }

        #endregion
    }
}

