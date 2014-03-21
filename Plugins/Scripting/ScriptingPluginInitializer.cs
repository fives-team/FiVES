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

        public string Name
        {
            get
            {
                return "Scripting";
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
            scripting.AddAttribute<string>("serverScriptDeps");
            scripting.AddAttribute<string>("clientScript");
            ComponentRegistry.Instance.Register(scripting);

            Scripting.Instance = new ScriptingImpl();
        }

        public void Shutdown()
        {
        }

        #endregion
    }
}

