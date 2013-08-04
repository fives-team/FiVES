using System;
using FIVES;
using KIARA;

namespace WebSocketJSON
{
    public class WebSocketJSON : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string getName()
        {
            return "WebSocketJSON";
        }

        public System.Collections.Generic.List<string> getDependencies()
        {
            return new System.Collections.Generic.List<string>();
        }

        public void initialize()
        {
            ProtocolRegistry.Instance.registerProtocolFactory("websocket-json", new WSJProtocolFactory());
        }

        #endregion
    }
}

