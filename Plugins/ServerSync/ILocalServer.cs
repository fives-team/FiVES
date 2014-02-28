using KIARAPlugin;
using System;

namespace ServerSyncPlugin
{
    public interface ILocalServer
    {
        // KIARA service on the local service.
        IServiceImpl Service { get; }

        // Local domain-of-reponsibility.
        IDomainOfResponsibility DoR { get; set; }

        // Local domain-of-interest.
        IDomainOfInterest DoI { get; set; }

        // Local SyncID.
        Guid SyncID { get; }

        // Events which are triggered when the local DoI or DoR has changed.
        event EventHandler DoIChanged;
        event EventHandler DoRChanged;
    }
}
