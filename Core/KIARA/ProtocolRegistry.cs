using System;
using System.Collections.Generic;

namespace KIARA
{
    public class ProtocolRegistry
    {
        public static ProtocolRegistry Instance = new ProtocolRegistry();

        // Registers a protocol |factory| for the |protocol|.
        public void registerProtocolFactory(string protocol, IProtocolFactory factory)
        {
            if (protocol == null)
                throw new Error(ErrorCode.INVALID_VALUE, "Protocol name must not be null.");

            if (isRegistered(protocol))
                throw new Error(ErrorCode.INVALID_VALUE, "Protocol " + protocol + " is already registered.");

            registeredProtocols[protocol] = factory;
        }

        // Returns protocol factory for |protocol| or null if |protocol| is not registered.
        public IProtocolFactory getProtocolFactory(string protocol) {
            if (isRegistered(protocol))
                return registeredProtocols[protocol];
            return null;
        }

        // Returns whether a factory for the |protocol| is registered.
        public bool isRegistered(string protocol) {
            return registeredProtocols.ContainsKey(protocol);
        }

        private Dictionary<string, IProtocolFactory> registeredProtocols = new Dictionary<string, IProtocolFactory>();
    }
}

