using FIVES;
using System;
using System.Runtime.Serialization;

namespace ServerSyncPlugin
{
    public interface IDomainOfResponsibility : ISerializable
    {
        /// <summary>
        /// Checks if this DoR includes a given entity.
        /// </summary>
        /// <param name="entity">A given entity.</param>
        /// <returns>True if the DoR contains a given entity, false otherwise.</returns>
        bool IsResponsibleFor(Entity entity);

        /// <summary>
        /// Triggered when this DoR changes.
        /// </summary>
        event EventHandler Changed;
    }
}
