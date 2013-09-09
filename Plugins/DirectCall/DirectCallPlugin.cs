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
            // Register the protocol factory in the protocol registry.
            ProtocolRegistry.Instance.registerProtocolFactory("direct-call", new DCProtocolFactory());

            // Create inter-plugin context and initialize it with this protocol.
            Context interPluginContext = ContextFactory.getContext("inter-plugin");
            interPluginContext.initialize("{{servers:[{{protocol:{{name:'direct-call', id:'{0}'}}}}]}}");
        }

        #endregion
    }
}

