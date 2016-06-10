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
