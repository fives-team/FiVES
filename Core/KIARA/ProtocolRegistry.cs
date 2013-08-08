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

    /// <summary>
    /// Protocol registry. Allows to register new protocol types and their implementations.
    /// </summary>
    public class ProtocolRegistry : IProtocolRegistry
    {
        /// <summary>
        /// Default instance of the protocol registry. This should be used instead of creating a new instance.
        /// </summary>
        public readonly static ProtocolRegistry Instance = new ProtocolRegistry();

        /// <summary>
        /// Registers a protocol <paramref name="factory"/> for the <paramref name="protocol"/>.
        /// </summary>
        /// <param name="protocol">Protocol name.</param>
        /// <param name="factory">Protocol factory.</param>
        public void registerProtocolFactory(string protocol, IProtocolFactory factory)
        {
            if (protocol == null)
                throw new Error(ErrorCode.INVALID_VALUE, "Protocol name must not be null.");

            if (isRegistered(protocol))
                throw new Error(ErrorCode.INVALID_VALUE, "Protocol " + protocol + " is already registered.");

            registeredProtocols[protocol] = factory;
        }

        /// <summary>
        /// Returns protocol factory for <paramref name="protocol"/>. If protocol is not registered, an exception is
        /// thrown.
        /// </summary>
        /// <returns>The protocol factory.</returns>
        /// <param name="protocol">Protocol name.</param>
        public IProtocolFactory getProtocolFactory(string protocol) {
            if (isRegistered(protocol))
                return registeredProtocols[protocol];
            throw new Error(ErrorCode.GENERIC_ERROR, "Protocol " + protocol + " is not registered.");
        }

        /// <summary>
        /// Returns whether a factory for the <paramref name="protocol"/> is registered.
        /// </summary>
        /// <returns><c>true</c>, if <paramref name="protocol"/> is registered, <c>false</c> otherwise.</returns>
        /// <param name="protocol">Protocol name.</param>
        public bool isRegistered(string protocol) {
            return registeredProtocols.ContainsKey(protocol);
        }

        private Dictionary<string, IProtocolFactory> registeredProtocols = new Dictionary<string, IProtocolFactory>();
    }
}

