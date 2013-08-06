using System;

namespace KIARA
{
    public interface IProtocolFactory
    {
        void openConnection(Server serverConfig, Action<IProtocol> onConnected);
        void startServer(Server serverConfig, Action<IProtocol> onNewClient);
    }
}

