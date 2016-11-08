using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiagnosisPlugin
{
    class ExtendableDiagnosisInterface
    {
        PluginContext IntroduceSelf(IDiagnosablePlugin self)
        {
            PluginContext context = new PluginContext(self);
            registeredPlugins.Add(self.Name, context);
            return context;
        }

        // POST /diagnosis/action/$PLUGINNAME/$METHODNAME ; [params]
        void CallPluginMethod(string pluginName, string methodName, object[] parameters)
        {
            registeredPlugins[pluginName].CallMethod(methodName, parameters);
        }

        Dictionary<string, PluginContext> registeredPlugins = new Dictionary<string, PluginContext>();
    }
}
