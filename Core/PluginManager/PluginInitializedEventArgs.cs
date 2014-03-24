using System;

namespace FIVES
{
    public class PluginInitializedEventArgs : EventArgs
    {
        public PluginInitializedEventArgs(string pluginName)
        {
            PluginName = pluginName;
        }

        public string PluginName { get; private set; }
    }
}

