using FIVES;
using System;
using System.Runtime.Serialization;

namespace ServerSyncPlugin
{
    [Serializable]
    class EmptyDoR : IDomainOfResponsibility
    {
        /// <summary>
        /// Constructs a EmptyDoR object.
        /// </summary>
        public EmptyDoR()
        {
        }

        /// <summary>
        /// Checks if this DoR includes a given entity.
        /// </summary>
        /// <param name="entity">A given entity.</param>
        /// <returns>True if the DoR contains a given entity, false otherwise.</returns>
        public bool IsResponsibleFor(Entity entity)
        {
            return false;
        }

        /// <summary>
        /// Triggered when this DoR changes.
        /// </summary>
        public event EventHandler Changed
        {
            add { }
            remove { }
        }

        #region ISerializable interface

        public EmptyDoR(SerializationInfo info, StreamingContext context)
        {
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
        }

        #endregion
    }
}
