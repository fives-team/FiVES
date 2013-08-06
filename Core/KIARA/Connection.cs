using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;

namespace KIARA
{
    // This class represenents a connection to the remote end. Can be used to generate remote function wrappers and to
    // register func implementations on the local end.
    public class Connection
    {
        // Func wrapper delegate generated with generateFuncWrapper. Allows passing arbitrary arguments and returns an
        // object representing the remote call.
        public delegate IFuncCall FuncWrapper(params object[] args);

        // Constructs connection from a protocol. Please use Context.openConnection or Context.startServer to construct
        // a new connection.
        internal Connection(IProtocol aProtocol) : this(aProtocol, new WebClientWrapper()) {}

        // Loads an IDL at |uri| into the connection.
        public void loadIDL(string uri)
        {
            string contents = webClient.DownloadString(uri);
            // TODO: Parse the IDL and pass parsed structure into processIDL.
            protocol.processIDL(contents);
        }

        // Generates a func wrapper for the |funcName|. Optional |typeMapping| can be used to specify data omission and
        // reordering options.
        public FuncWrapper generateFuncWrapper(string funcName, string typeMapping = "")
        {
            // TODO: implement type mapping and add respective tests
            return (FuncWrapper) delegate(object[] args) {
                return protocol.callFunc(funcName, args);
            };
        }

        // Registers a local |handler| as an implementation for the |funcName|. Optional |typeMapping| can be used to
        // specify data omission and reordering options.
        public void registerFuncImplementation(string funcName, Delegate handler, string typeMapping = "")
        {
            // TODO: implement type mapping and add respective tests
            protocol.registerHandler(funcName, handler);
        }

        private IProtocol protocol;
        private IWebClient webClient;

        #region Testing

        internal Connection(IProtocol aProtocol, IWebClient client)
        {
            protocol = aProtocol;
            webClient = client;
        }

        #endregion
    }
}
