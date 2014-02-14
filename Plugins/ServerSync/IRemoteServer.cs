using KIARAPlugin;
using System;

namespace ServerSyncPlugin
{
    public interface IRemoteServer
    {
        // KIARA connection to the remote server.
        Connection Connection { get; }

        // Remote domain-of-reponsibility.
        IDomainOfResponsibility DoR { get; }

        // Remote domain-of-interest.
        IDomainOfInterest DoI { get; }

        // Remote SyncID.
        Guid SyncID { get; }

        // Events which are triggered when the remote DoI or DoR has changed.
        event EventHandler DoIChanged;
        event EventHandler DoRChanged;
    }
}
