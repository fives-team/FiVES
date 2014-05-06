// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using KIARAPlugin;
using WebSocket4Net;
using SuperSocket.ClientEngine;

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

    public interface IWebSocketFactory
    {
        ISocket Construct(string uri);
    }

    public class WebSocketFactory : IWebSocketFactory
    {
        public ISocket Construct(string uri)
        {
            return new WebSocketSocketAdapter(uri);
        }
    }
}

