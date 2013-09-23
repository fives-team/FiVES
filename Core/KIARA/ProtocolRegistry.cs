using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using System.Reflection;

namespace KIARA
{
    #region Testing
    public interface IProtocolRegistry
    {
        void RegisterProtocolFactory(string protocol, IProtocolFactory factory);
        IProtocolFactory GetProtocolFactory(string protocol);
        bool IsRegistered(string protocol);
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
        public void RegisterProtocolFactory(string protocol, IProtocolFactory factory)
        {
            if (protocol == null)
                throw new Error(ErrorCode.INVALID_VALUE, "Protocol name must not be null.");

            if (IsRegistered(protocol))
                throw new Error(ErrorCode.INVALID_VALUE, "Protocol " + protocol + " is already registered.");

            registeredProtocols[protocol] = factory;
        }

        /// <summary>
        /// Returns protocol factory for <paramref name="protocol"/>. If protocol is not registered, an exception is
        /// thrown.
        /// </summary>
        /// <returns>The protocol factory.</returns>
        /// <param name="protocol">Protocol name.</param>
        public IProtocolFactory GetProtocolFactory(string protocol) {
            if (IsRegistered(protocol))
                return registeredProtocols[protocol];
            throw new Error(ErrorCode.GENERIC_ERROR, "Protocol " + protocol + " is not registered.");
        }

        /// <summary>
        /// Returns whether a factory for the <paramref name="protocol"/> is registered.
        /// </summary>
        /// <returns><c>true</c>, if <paramref name="protocol"/> is registered, <c>false</c> otherwise.</returns>
        /// <param name="protocol">Protocol name.</param>
        public bool IsRegistered(string protocol) {
            return registeredProtocols.ContainsKey(protocol);
        }

        public void LoadProtocolsFrom(string protocolDir)
        {
            string[] files = Directory.GetFiles(protocolDir, "*.dll");
            foreach (string filename in files)
                LoadProtocol(filename);
        }

        void LoadProtocol(string filename)
        {
            try {
                // Load an assembly.
                Assembly assembly = Assembly.LoadFrom(filename);

                // Find protocol factory (class implementing IProtocolFactory).
                List<Type> types = new List<Type>(assembly.GetTypes());
                Type protocolFactoryType = types.Find(t => typeof(IProtocolFactory).IsAssignableFrom(t));
                if (protocolFactoryType == null) {
                    Logger.Warn("Assembly in file " + filename +
                                " doesn't contain any class implementing IProtocolFactory.");
                    return;
                }

                // Instantiate and register protocol factory.
                var protocolFactory = (IProtocolFactory)Activator.CreateInstance(protocolFactoryType);
                RegisterProtocolFactory(protocolFactory.GetName(), protocolFactory);
            } catch (Exception e) {
                Logger.WarnException("Failed to load file " + filename + " as a protocol.", e);
                return;
            }
        }

        Dictionary<string, IProtocolFactory> registeredProtocols = new Dictionary<string, IProtocolFactory>();

        static Logger Logger = LogManager.GetCurrentClassLogger();
    }
}

