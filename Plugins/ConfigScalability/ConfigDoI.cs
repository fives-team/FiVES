// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
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

        public event EventHandler Changed;

        #region ISerializable interface

        public ConfigDoI(SerializationInfo info, StreamingContext context)
        {
            minX = info.GetDouble("minX");
            maxX = info.GetDouble("maxX");
            minY = info.GetDouble("minY");
            maxY = info.GetDouble("maxY");
            relevantComponents = (List<string>)info.GetValue("relevantComponents", typeof(List<string>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("minX", minX);
            info.AddValue("maxX", maxX);
            info.AddValue("minY", minY);
            info.AddValue("maxY", maxY);
            info.AddValue("relevantComponents", relevantComponents);
        }

        #endregion

        internal double MinX
        {
            get
            {
                return minX;
            }
            set
            {
                minX = value;
                if (Changed != null)
                    Changed(this, new EventArgs());
            }
        }

        internal double MaxX
        {
            get
            {
                return maxX;
            }
            set
            {
                maxX = value;
                if (Changed != null)
                    Changed(this, new EventArgs());
            }
        }

        internal double MinY
        {
            get
            {
                return minY;
            }
            set
            {
                minY = value;
                if (Changed != null)
                    Changed(this, new EventArgs());
            }
        }

        internal double MaxY
        {
            get
            {
                return maxY;
            }
            set
            {
                maxY = value;
                if (Changed != null)
                    Changed(this, new EventArgs());
            }
        }

        internal List<string> RelevantComponents
        {
            get
            {
                return relevantComponents;
            }
            set
            {
                relevantComponents = value;
                if (Changed != null)
                    Changed(this, new EventArgs());
            }
        }

        private double minX;
        private double maxX;
        private double minY;
        private double maxY;
        private List<string> relevantComponents;
    }
}
