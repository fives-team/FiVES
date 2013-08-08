using System;
using FIVES;
using KIARA;

namespace WebSocketJSON
{
    /// <summary>
    /// Plugin that registers "direct-call" (DC) protocol in KIARA. It uses JSON-based protocol on top of WebSocket.
    /// It is a simple protocol mainly developed for testing and was not designed for real virtual world applications.
    /// </summary>
    public class WebSocketJSONPlugin : IPluginInitializer
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

