using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;

namespace KIARAPlugin
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
    public abstract class Connection
    {
        /// <summary>
        /// Raised when a connection is closed.
        /// </summary>
        public abstract event EventHandler Closed;

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// Convenient wrapper around GenerateFuncWrapper. Can be used to quickly create function wrapper and call it
        /// at once, e.g. <c>client["clientFunc"]("arg1", 42);</c>
        /// </summary>
        /// <param name="name">Function name.</param>
        /// <returns>Function wrapper.</returns>
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
        public virtual void LoadIDL(string uri)
        {
            string contents = webClient.DownloadString(uri);
            // TODO: Parse the IDL and pass parsed structure into ProcessIDL.
            ProcessIDL(contents);
        }

        /// <summary>
        /// Generates a func wrapper for the <paramref name="funcName"/>. Optional <paramref name="typeMapping"/> string
        /// may be used to specify data omission and reordering options.
        /// </summary>
        /// <returns>The generated func wrapper.</returns>
        /// <param name="funcName">Name of the function to be wrapped.</param>
        /// <param name="typeMapping">Type mapping string.</param>
        public virtual FuncWrapper GenerateFuncWrapper(string funcName, string typeMapping = "")
        {
            // TODO: implement type mapping and add respective tests
            return (FuncWrapper) delegate(object[] args) {
                return CallFunc(funcName, args);
            };
        }

        /// <summary>
        /// Registers a local <paramref name="handler"/> as an implementation for the <paramref name="funcName"/>.
        /// Optional <paramref name="typeMapping"/> string can be used to specify data omission and reordering options.
        /// </summary>
        /// <param name="funcName">Name of the implemented function.</param>
        /// <param name="handler">Handler to be invoked upon remote call.</param>
        /// <param name="typeMapping">Type mapping string.</param>
        public virtual void RegisterFuncImplementation(string funcName, Delegate handler, string typeMapping = "")
        {
            // TODO: implement type mapping and add respective tests
            RegisterHandler(funcName, handler);
        }

        /// <summary>
        /// Sets some property of the connection. May be used by subclasses to allow clients to configure them.
        /// </summary>
        /// <returns><c>true</c>, if property is supported and accepts given value, <c>false</c> otherwise.</returns>
        /// <param name="name">Name of the property.</param>
        /// <param name="value">Value to be set.</param>
        public virtual bool SetProperty(string name, object value)
        {
            return false;
        }

        /// <summary>
        /// Returns some property of the connection. May be used by subclasses to allow clients obtain some information
        /// about the connection.
        /// </summary>
        /// <returns><c>true</c>, if property is supported, <c>false</c> otherwise.</returns>
        /// <param name="name">Name of the property.</param>
        /// <param name="value">Value to be returned.</param>
        public virtual bool GetProperty(string name, out object value)
        {
            value = null;
            return false;
        }

        /// <summary>
        /// Processes the parsed IDL.
        /// </summary>
        /// <param name="parsedIDL">Plain IDL as string (parsing is not implemented yet).</param>
        protected abstract void ProcessIDL(string parsedIDL);

        /// <summary>
        /// Calls a function with a given name and arguments on the remote end.
        /// </summary>
        /// <param name="funcName">Function name.</param>
        /// <param name="args">Argunents.</param>
        /// <returns>Object representing remote call.</returns>
        protected abstract IFuncCall CallFunc(string funcName, params object[] args);

        /// <summary>
        /// Registers a handler to be invoked when a function with given name is invoked remotely.
        /// </summary>
        /// <param name="funcName">Function name.</param>
        /// <param name="handler">Handler delegate.</param>
        protected abstract void RegisterHandler(string funcName, Delegate handler);

        internal IWebClient webClient = new WebClientWrapper();
    }
}
