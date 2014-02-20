using FIVES;
using System;
using System.Runtime.Serialization;

namespace ServerSyncPlugin
{
    [Serializable]
    class EmptyDoR : IDomainOfResponsibility
    {
        public EmptyDoR()
        {
        }

        public bool IsResponsibleFor(Entity entity)
        {
            return false;
        }

#pragma warning disable 67
        public event EventHandler Changed;
#pragma warning restore 67

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
