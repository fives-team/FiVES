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

        public bool IsInterestedInEntity(Entity entity)
        {
            return false;
        }

        public bool IsInterestedInAttributeChange(Entity entity, string componentName, string attributeName)
        {
            return false;
        }

#pragma warning disable 67
        public event EventHandler Changed;
#pragma warning restore 67

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
