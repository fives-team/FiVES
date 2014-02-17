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

        public bool IsInterestedInEntity(Guid guid)
        {
            return false;
        }

        public bool IsInterestedInAttributeChange(Guid entityGuid, string componentName, string attributeName)
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
