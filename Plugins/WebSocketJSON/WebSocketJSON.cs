using System;
using FIVES;
using KIARA;

namespace WebSocketJSON
{
    public class WebSocketJSON : IPluginInitializer
    {
        public WebSocketJSON()
        {
            ProtocolRegistry.Instance.registerProtocolFactory("websocket-json", new WSJProtocolFactory());
        }
    }
}

