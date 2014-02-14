using FIVES;
using System.Runtime.Serialization;

namespace ServerSyncPlugin
{
    public interface IDomainOfResponsibility : ISerializable
    {
        // Checks if this DoR includes a given entity.
        bool IsResponsibleFor(Entity entity);
    }
}
