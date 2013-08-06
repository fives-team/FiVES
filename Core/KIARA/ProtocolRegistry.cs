using System;
using System.Collections.Generic;

namespace KIARA
{
    #region Testing
    public interface IProtocolRegistry
    {
        void registerProtocolFactory(string protocol, IProtocolFactory factory);
        IProtocolFactory getProtocolFactory(string protocol);
        bool isRegistered(string protocol);
    }
    #endregion

    public class ProtocolRegistry : IProtocolRegistry
    {
        public readonly static ProtocolRegistry Instance = new ProtocolRegistry();

        // Registers a protocol |factory| for the |protocol|.
        public void registerProtocolFactory(string protocol, IProtocolFactory factory)
        {
            if (protocol == null)
                throw new Error(ErrorCode.INVALID_VALUE, "Protocol name must not be null.");

            if (isRegistered(protocol))
                throw new Error(ErrorCode.INVALID_VALUE, "Protocol " + protocol + " is already registered.");

            registeredProtocols[protocol] = factory;
        }

        // Returns protocol factory for |protocol|. If protocol is not registered, throws an exception.
        public IProtocolFactory getProtocolFactory(string protocol) {
            if (isRegistered(protocol))
                return registeredProtocols[protocol];
            throw new Error(ErrorCode.GENERIC_ERROR, "Protocol " + protocol + " is not registered.");
        }

        // Returns whether a factory for the |protocol| is registered.
        public bool isRegistered(string protocol) {
            return registeredProtocols.ContainsKey(protocol);
        }

        private Dictionary<string, IProtocolFactory> registeredProtocols = new Dictionary<string, IProtocolFactory>();
    }
}

