using System;

namespace FIVES
{
    public class PluginInitializedEventArgs : EventArgs
    {
        public PluginInitializedEventArgs (string pluginName)
        {
            this.pluginName = pluginName;
        }

        public string pluginName;
    }
}

