using System;
using KIARA;

namespace WebSocketJSON
{
    public interface IWSJServerFactory
    {
        IWSJServer construct(Action<IProtocol> onNewClient);
    }

    public class WSJServerFactory : IWSJServerFactory
    {
        public IWSJServer construct(Action<IProtocol> onNewClient)
        {
            return new WSJServer(onNewClient);
        }
    }

    public interface IWSJFuncCallFactory
    {
        IWSJFuncCall construct();
    }

    public class WSJFuncCallFactory : IWSJFuncCallFactory
    {
        public IWSJFuncCall construct()
        {
            return new WSJFuncCall();
        }
    }
}

