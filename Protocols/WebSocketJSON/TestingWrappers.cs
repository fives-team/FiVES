using System;
using KIARAPlugin;

namespace WebSocketJSON
{
    public interface IWSJServerFactory
    {
        IWSJServer Construct(Action<Connection> onNewClient);
    }

    public class WSJServerFactory : IWSJServerFactory
    {
        public IWSJServer Construct(Action<Connection> onNewClient)
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

