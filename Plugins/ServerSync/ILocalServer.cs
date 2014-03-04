using KIARAPlugin;
using System;

namespace ServerSyncPlugin
{
    /// <summary>
    /// Represents a local server.
    /// </summary>
    public interface ILocalServer
    {
        /// <summary>
        /// KIARA service on the local service.
        /// </summary>
        IServiceImpl Service { get; }

        /// <summary>
        /// Local domain-of-reponsibility.
        /// </summary>
        IDomainOfResponsibility DoR { get; set; }

        /// <summary>
        /// Local domain-of-interest.
        /// </summary>
        IDomainOfInterest DoI { get; set; }

        /// <summary>
        /// Local SyncID.
        /// </summary>
        Guid SyncID { get; }

        /// <summary>
        /// Triggered when the local DoI has changed.
        /// </summary>
        event EventHandler DoIChanged;

        /// <summary>
        /// Triggered when the local DoR has changed.
        /// </summary>
        event EventHandler DoRChanged;
    }
}
