using FIVES;
using ServerSyncPlugin;
using System;
using System.Runtime.Serialization;

namespace ConfigScalabilityPlugin
{
    [Serializable]
    class ConfigDoR : IDomainOfResponsibility
    {
        public ConfigDoR()
        {
        }

        public bool IsResponsibleFor(Entity entity)
        {
            Vector position = (Vector)entity["location"]["position"];
            return position.x >= MinX && position.x < MaxX && position.y >= MinY && position.y < MaxY;
        }

        public override string ToString()
        {
            return String.Format("minX = {0}, maxX = {1}, minY = {2}, maxY = {3}", MinX, MaxX, MinY, MaxY);
        }

        #region ISerializable interface

        public ConfigDoR(SerializationInfo info, StreamingContext context)
        {
            MinX = info.GetDouble("minX");
            MaxX = info.GetDouble("maxX");
            MinY = info.GetDouble("minY");
            MaxY = info.GetDouble("maxY");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("minX", MinX);
            info.AddValue("maxX", MaxX);
            info.AddValue("minY", MinY);
            info.AddValue("maxY", MaxY);
        }

        #endregion

        internal double MinX;
        internal double MaxX;

        internal double MinY;
        internal double MaxY;
    }
}
