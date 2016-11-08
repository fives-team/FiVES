using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiagnosisPlugin
{
    class DiagnosisInterface
    {
        public static DiagnosisInterface Instance = new DiagnosisInterface();

        public void IntroducePlugin(IPluginInitializer pluginInitializer)
        {
            registeredPlugins.Add(pluginInitializer.Name);
        }

        public void IntroduceValue(string valueName, object value)
        {
            registeredValues.Add(valueName, value);
        }

        public void IntroduceRPC(string methodName, Delegate handler)
        {
            registeredRPCs.Add(methodName, handler);
        }

        public List<string> registeredPlugins { get; private set; } = new List<string>();
        public Dictionary<string, object> registeredValues { get; private set; } = new Dictionary<string, object>();
        public Dictionary<string, Delegate> registeredRPCs { get; private set; } = new Dictionary<string, Delegate>();
    }
}
