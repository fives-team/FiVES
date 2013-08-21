using System;

namespace Events
{
    public class PluginLoadedEventArgs : EventArgs
    {
        public PluginLoadedEventArgs (string pluginName)
        {
            this.pluginName = pluginName;
        }

        public string pluginName;
    }
}

