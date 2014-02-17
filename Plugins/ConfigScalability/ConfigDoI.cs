using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServerSyncPlugin;
using System.Runtime.Serialization;
using FIVES;

namespace ConfigScalabilityPlugin
{
    [Serializable]
    class ConfigDoI : IDomainOfInterest
    {
        public bool IsInterestedInEntity(Entity entity)
        {
            Vector position = (Vector)entity["location"]["position"];
            return position.x >= MinX && position.x < MaxX && position.y >= MinY && position.y < MaxY;
        }

        public bool IsInterestedInAttributeChange(Entity entity, string componentName, string attributeName)
        {
            return IsInterestedInEntity(entity) && RelevantComponents.Contains(componentName);
        }

        #region ISerializable interface

        public ConfigDoI(SerializationInfo info, StreamingContext context)
        {
            MinX = info.GetSingle("minX");
            MaxX = info.GetSingle("maxX");
            MinY = info.GetSingle("minY");
            MaxY = info.GetSingle("maxY");
            RelevantComponents = (List<string>)info.GetValue("relevantComponents", typeof(List<string>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("minX", MinX);
            info.AddValue("maxX", MaxX);
            info.AddValue("minY", MinY);
            info.AddValue("maxY", MaxY);
            info.AddValue("relevantComponents", RelevantComponents);
        }

        #endregion

        private float MinX;
        private float MaxX;

        private float MinY;
        private float MaxY;

        private List<string> RelevantComponents;
    }
}
