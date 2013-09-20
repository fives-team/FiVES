using System;
using KIARA;

namespace WebSocketJSON
{
    public interface IWSJServerFactory
    {
        IWSJServer Construct(Action<IProtocol> onNewClient);
    }

    public class WSJServerFactory : IWSJServerFactory
    {
        public IWSJServer Construct(Action<IProtocol> onNewClient)
        {
            return new WSJServer(onNewClient);
        }
    }

    public interface IWSJFuncCallFactory
    {
        IWSJFuncCall Construct();
    }

    public class WSJFuncCallFactory : IWSJFuncCallFactory
    {
        public IWSJFuncCall Construct()
        {
            return new WSJFuncCall();
        }
    }
}

