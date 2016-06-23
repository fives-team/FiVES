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

namespace ServerSyncPlugin
{
    /// <summary>
    /// Contains a collection of attribute sync info for all attributes in one component.
    /// </summary>
    class ComponentSyncInfo
    {
        /// <summary>
        /// Convenience operator for getting or setting attribute sync info. Allows to use square brackets without the
        /// need to write the Attributes property name: componentSyncInfo["attr"].
        /// </summary>
        /// <param name="attributeName">Attribute name.</param>
        /// <returns>Relevant sync info.</returns>
        public AttributeSyncInfo this[string attributeName]
        {
            get
            {
                return Attributes[attributeName];
            }
            set
            {
                Attributes[attributeName] = value;
            }
        }

        /// <summary>
        /// A collection of component's attribute sync info. Key is attribute name.
        /// </summary>
        public Dictionary<string, AttributeSyncInfo> Attributes = new Dictionary<string, AttributeSyncInfo>();
    }
}
