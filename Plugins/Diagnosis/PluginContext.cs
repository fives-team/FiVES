using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DiagnosisPlugin
{
    public class PluginContext
    {
        public IDiagnosablePlugin Plugin { get; private set; }

        public PluginContext(IDiagnosablePlugin plugin)
        {
            this.Plugin = plugin;
        }

        public PluginWidget GetPluginWidget()
        {
            return Plugin.Widget;
        }

        public void RegisterMethod(string methodName, Action<string[]> handler)
        {            
            methodHandlers.Add(methodName, handler);
        }

        // Not sure if this should really be public
        public bool CallMethod(string methodName, string[] parameters)
        {
            methodHandlers[methodName].Invoke(parameters);
            // TODO: Was genau?! Bool on success
            return true;
        }

        Dictionary<string, Action<string[]>> methodHandlers = new Dictionary<string, Action<string[]>>();
    }
}
