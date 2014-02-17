using FIVES;
using System;
using System.Runtime.Serialization;

namespace ServerSyncPlugin
{
    public interface IDomainOfInterest : ISerializable
    {
        // Checks if this DoI includes a given entity.
        bool IsInterestedInEntity(Guid entityGuid);

        // Checks if this DoI includes a given attribute changed event.
        bool IsInterestedInAttributeChange(Guid entityGuid, string componentName, string attributeName);
    }
}
