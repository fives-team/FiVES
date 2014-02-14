using FIVES;
using System.Runtime.Serialization;

namespace ServerSyncPlugin
{
    public interface IDomainOfInterest : ISerializable
    {
        // Checks if this DoI includes a given entity.
        bool IsInterestedInEntity(EntityEventArgs args);

        // Checks if this DoI includes a given attribute changed event.
        bool IsInterestedInAttributeChange(ChangedAttributeEventArgs args);
    }
}
