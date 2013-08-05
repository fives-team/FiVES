using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;

namespace KIARA
{
    public class Connection
    {
        public delegate IFuncCall FuncWrapper(params object[] args);

        public Connection(IProtocol aProtocol)
        {
            protocol = aProtocol;
        }

        public void loadIDL(string uri)
        {
            WebClient client = new WebClient();
            string contents = client.DownloadString(uri);
            // TODO: Parse the IDL and pass parsed structure into processIDL.
            protocol.processIDL(contents);
        }

        public FuncWrapper generateFuncWrapper(string funcName, string typeMapping = "")
        {
            return (FuncWrapper) delegate(object[] args) {
                return protocol.callFunc(funcName, args);
            };
        }

        public void registerFuncImplementation(string funcName, Delegate handler, string typeMapping = "")
        {
            protocol.registerHandler(funcName, handler);
        }

        IProtocol protocol;
    }
}
