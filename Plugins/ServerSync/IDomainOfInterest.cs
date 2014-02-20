using FIVES;
using System;
using System.Runtime.Serialization;

namespace ServerSyncPlugin
{
    public interface IDomainOfInterest : ISerializable
    {
        // Checks if this DoI includes a given entity.
        bool IsInterestedInEntity(Entity entity);

        // Checks if this DoI includes a given attribute changed event.
        bool IsInterestedInAttributeChange(Entity entity, string componentName, string attributeName);

        event EventHandler Changed;
    }
}
