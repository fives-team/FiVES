using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using System.Reflection;

namespace KIARAPlugin
{
    #region Testing
    public interface IProtocolRegistry
    {
        void RegisterConnectionFactory(string protocol, IConnectionFactory factory);
        IConnectionFactory GetConnectionFactory(string protocol);
        bool IsRegistered(string protocol);
    }
    #endregion

    /// <summary>
    /// Protocol registry. Allows to register new protocol types and their implementations.
    /// </summary>
    public class ProtocolRegistry : IProtocolRegistry
    {
        /// <summary>
        /// Default instance of the protocol registry.
        /// </summary>
        public readonly static ProtocolRegistry Instance = new ProtocolRegistry();

        /// <summary>
        /// Registers a connection <paramref name="factory"/> for the <paramref name="protocol"/>.
        /// </summary>
        /// <param name="protocol">Protocol name.</param>
        /// <param name="factory">Connection factory.</param>
        public void RegisterConnectionFactory(string protocol, IConnectionFactory factory)
        {
            if (protocol == null)
                throw new Error(ErrorCode.INVALID_VALUE, "Protocol name must not be null.");

            if (IsRegistered(protocol))
                throw new Error(ErrorCode.INVALID_VALUE, "Protocol " + protocol + " is already registered.");

            registeredProtocols[protocol] = factory;
        }

        /// <summary>
        /// Returns connection factory for <paramref name="protocol"/>. If protocol is not registered, an exception is
        /// thrown.
        /// </summary>
        /// <returns>The protocol factory.</returns>
        /// <param name="protocol">Connection name.</param>
        public IConnectionFactory GetConnectionFactory(string protocol) {
            if (IsRegistered(protocol))
                return registeredProtocols[protocol];
            throw new Error(ErrorCode.GENERIC_ERROR, "Protocol " + protocol + " is not registered.");
        }

        /// <summary>
        /// Returns whether a given <paramref name="protocol"/> is registered.
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

                // Find connection factory (class implementing IConnectionFactory).
                List<Type> types = new List<Type>(assembly.GetTypes());
                Type interfaceType = typeof(IConnectionFactory);
                Type connectionFactoryType = types.Find(t => interfaceType.IsAssignableFrom(t));
                if (connectionFactoryType == null || connectionFactoryType.Equals(interfaceType)) {
                    Logger.Info("Assembly in file " + filename +
                                " doesn't contain any class implementing IConnectionFactory.");
                    return;
                }

                // Instantiate and register protocol factory.
                IConnectionFactory connectionFactory;
                try {
                    connectionFactory = (IConnectionFactory)Activator.CreateInstance(connectionFactoryType);
                } catch (Exception ex) {
                    Logger.WarnException("Exception occured during construction of protocol factory for " + filename + ".", ex);
                    return;
                }
                RegisterConnectionFactory(connectionFactory.GetName(), connectionFactory);
                Logger.Debug("Registered protocol {0}", connectionFactory.GetName());
            } catch (BadImageFormatException e) {
                Logger.InfoException(filename + " is not a valid assembly and thus cannot be loaded as a protocol.", e);
                return;
            } catch (Exception e) {
                Logger.WarnException("Failed to load file " + filename + " as a protocol", e);
                return;
            }
        }

        Dictionary<string, IConnectionFactory> registeredProtocols = new Dictionary<string, IConnectionFactory>();

        static Logger Logger = LogManager.GetCurrentClassLogger();
    }
}

