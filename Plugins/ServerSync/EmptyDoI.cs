using FIVES;
using System;
using System.Runtime.Serialization;

namespace ServerSyncPlugin
{
    [Serializable]
    class EmptyDoI : IDomainOfInterest
    {
        public EmptyDoI()
        {
        }

        public bool IsInterestedInEntity(EntityEventArgs args)
        {
            return false;
        }

        public bool IsInterestedInAttributeChange(ChangedAttributeEventArgs args)
        {
            return false;
        }

        #region ISerializable interface

        public EmptyDoI(SerializationInfo info, StreamingContext context)
        {
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
        }

        #endregion
    }
}
