using FIVES;
using ServerSyncPlugin;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ConfigScalabilityPlugin
{
    [Serializable]
    class ConfigDoI : IDomainOfInterest
    {
        public ConfigDoI()
        {
        }

        public bool IsInterestedInEntity(Entity entity)
        {
            Vector position = (Vector)entity["location"]["position"];
            return position.x >= MinX && position.x < MaxX && position.y >= MinY && position.y < MaxY;
        }

        public bool IsInterestedInAttributeChange(Entity entity, string componentName, string attributeName)
        {
            return RelevantComponents == null || RelevantComponents.Contains(componentName);
        }

        public override string ToString()
        {
            return String.Format("minX = {0}, maxX = {1}, minY = {2}, maxY = {3}, components = ({4})",
                MinX, MaxX, MinY, MaxY, RelevantComponents == null ? "none" : String.Join(",", RelevantComponents));
        }

        #region ISerializable interface

        public ConfigDoI(SerializationInfo info, StreamingContext context)
        {
            MinX = info.GetDouble("minX");
            MaxX = info.GetDouble("maxX");
            MinY = info.GetDouble("minY");
            MaxY = info.GetDouble("maxY");
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

        internal double MinX;
        internal double MaxX;

        internal double MinY;
        internal double MaxY;

        internal List<string> RelevantComponents;
    }
}
