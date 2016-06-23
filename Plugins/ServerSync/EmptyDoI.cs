// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using FIVES;
using System;
using System.Runtime.Serialization;

namespace ServerSyncPlugin
{
    /// <summary>
    /// Implements an empty domain-of-interest, which is not interested in any entity or attribute change.
    /// </summary>
    [Serializable]
    class EmptyDoI : IDomainOfInterest
    {
        /// <summary>
        /// Constructs a EmptyDoI object.
        /// </summary>
        public EmptyDoI()
        {
        }

        /// <summary>
        /// Checks if this DoI includes a given entity.
        /// </summary>
        /// <param name="entity">A given entity.</param>
        /// <returns>True if the DoI contains a given entity, false otherwise.</returns>
        public bool IsInterestedInEntity(Entity entity)
        {
            return false;
        }

        /// <summary>
        /// Checks if this DoI includes a given attribute.
        /// </summary>
        /// <param name="entity">An entitycontaining the attribute, which was changed.</param>
        /// <param name="componentName">Name of the component containing the attribute, which was changed.</param>
        /// <param name="attributeName">Name of the changed attribute.</param>
        /// <returns>True DoI contains a given attribute, false otherwise.</returns>
        public bool IsInterestedInAttributeChange(Entity entity, string componentName, string attributeName)
        {
            return false;
        }

        /// <summary>
        /// Triggered when this DoI changes.
        /// </summary>
        public event EventHandler Changed
        {
            add { }
            remove { }
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
