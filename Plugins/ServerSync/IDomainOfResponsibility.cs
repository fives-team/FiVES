using FIVES;
using System;
using System.Runtime.Serialization;

namespace ServerSyncPlugin
{
    public interface IDomainOfResponsibility : ISerializable
    {
        // Checks if this DoR includes a given entity.
        bool IsResponsibleFor(Entity entity);

        // Triggered when this DoR changes.
        event EventHandler Changed;
    }
}
