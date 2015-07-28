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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
{
    /// <summary>
    /// Event arguments for ChangedAttribute event in Entity and Component classes.
    /// </summary>
    public class ChangedAttributeEventArgs : EventArgs
    {
        public ChangedAttributeEventArgs(Component component, string attributeName, object oldValue, object newValue)
        {
            Component = component;
            AttributeName = attributeName;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Returns containing entity. This is a shorthand for Component.ContainingEntity
        /// </summary>
        public Entity Entity {
            get
            {
                return Component.ContainingEntity;
            }
        }

        public Component Component { get; private set;  }
        public string AttributeName { get; private set; }
        public object OldValue { get; private set; }
        public object NewValue { get; private set; }
    }
}
