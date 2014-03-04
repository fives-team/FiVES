using KIARAPlugin;
using System;

namespace ServerSyncPlugin
{
    /// <summary>
    /// Represents a remote server.
    /// </summary>
    public interface IRemoteServer
    {
        /// <summary>
        /// KIARA connection to the remote server.
        /// </summary>
        Connection Connection { get; }

        /// <summary>
        /// Remote domain-of-reponsibility.
        /// </summary>
        IDomainOfResponsibility DoR { get; }

        /// <summary>
        /// Remote domain-of-interest.
        /// </summary>
        IDomainOfInterest DoI { get; }

        /// <summary>
        /// Remote SyncID.
        /// </summary>
        Guid SyncID { get; }

        /// <summary>
        /// Triggered when the remote DoI has changed.
        /// </summary>
        event EventHandler DoIChanged;

        /// <summary>
        /// Triggered when the remote DoR has changed.
        /// </summary>
        event EventHandler DoRChanged;
    }
}
