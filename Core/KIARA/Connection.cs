using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;

namespace KIARA
{
    /// <summary>
    /// Represents a generated function wrapper. It allows calling the function with arbitrary arguments.
    /// </summary>
    /// <returns>An object representing a call.</returns>
    public delegate IFuncCall FuncWrapper(params object[] args);

    /// <summary>
    /// This class represenents a connection to the remote end. It may be used to load new IDL definition files,
    /// generate callable remote function  wrappers and to register local functions as implementations for remote calls.
    /// </summary>
    public class Connection
    {
        /// <summary>
        /// Constructs connection given a protocol implementation. This for internal use only - please use
        /// <see cref="KIARA.Context.openConnection"/> or <see cref="KIARA.Context.startServer"/> instead.
        /// </summary>
        /// <param name="aProtocol">Protocol implementation.</param>
        internal Connection(IProtocol aProtocol) : this(aProtocol, new WebClientWrapper()) {}

        public event Close OnClose;

        public FuncWrapper this[string name]
        {
            get
            {
                return GenerateFuncWrapper(name);
            }
        }

        /// <summary>
        /// Loads an IDL definition file at <paramref name="uri"/> into the connection.
        /// </summary>
        /// <param name="uri">URI of the IDL definition file.</param>
        public void LoadIDL(string uri)
        {
            string contents = webClient.DownloadString(uri);
            // TODO: Parse the IDL and pass parsed structure into processIDL.
            protocol.ProcessIDL(contents);
        }

        /// <summary>
        /// Generates a func wrapper for the <paramref name="funcName"/>. Optional <paramref name="typeMapping"/> string
        /// may be used to specify data omission and reordering options.
        /// </summary>
        /// <returns>The generated func wrapper.</returns>
        /// <param name="funcName">Name of the function to be wrapped.</param>
        /// <param name="typeMapping">Type mapping string.</param>
        public FuncWrapper GenerateFuncWrapper(string funcName, string typeMapping = "")
        {
            // TODO: implement type mapping and add respective tests
            return (FuncWrapper) delegate(object[] args) {
                return protocol.CallFunc(funcName, args);
            };
        }

        /// <summary>
        /// Registers a local <paramref name="handler"/> as an implementation for the <paramref name="funcName"/>.
        /// Optional <paramref name="typeMapping"/> string can be used to specify data omission and reordering options.
        /// </summary>
        /// <param name="funcName">Name of the implemented function.</param>
        /// <param name="handler">Handler to be invoked upon remote call.</param>
        /// <param name="typeMapping">Type mapping string.</param>
        public void RegisterFuncImplementation(string funcName, Delegate handler, string typeMapping = "")
        {
            // TODO: implement type mapping and add respective tests
            protocol.RegisterHandler(funcName, handler);
        }

        public void Disconnect()
        {
            protocol.Disconnect();
        }

        private IProtocol protocol;
        private IWebClient webClient;

        #region Testing

        internal Connection(IProtocol aProtocol, IWebClient client)
        {
            protocol = aProtocol;
            webClient = client;

            protocol.OnClose += (reason) => { if (OnClose != null) OnClose(reason); };
        }

        #endregion
    }
}
