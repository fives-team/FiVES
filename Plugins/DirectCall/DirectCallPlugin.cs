using System;
using System.Collections.Generic;
using FIVES;
using KIARA;

namespace DirectCall
{
    /// <summary>
    /// Plugin that registers "direct-call" (DC) protocol in KIARA. Direct call protocol requires both parties to be in
    /// the same process and simply forwards calls without performing any serialization or deserialization.
    /// </summary>
    public class DirectCallPlugin : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string getName()
        {
            return "DirectCall";
        }

        public List<string> getDependencies()
        {
            return new List<string>();
        }

        public void initialize()
        {
            ProtocolRegistry.Instance.registerProtocolFactory("direct-call", new DCProtocolFactory());
        }

        #endregion
    }
}

