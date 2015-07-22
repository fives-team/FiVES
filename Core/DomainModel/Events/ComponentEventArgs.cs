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
    /// Event args for CreatedComponent event in Entity class and UpgradedComponent event in ComponentRegistry class.
    /// </summary>
    public class ComponentEventArgs : EventArgs
    {
        public ComponentEventArgs(Component component)
        {
            Component = component;
        }

        public Component Component { get; private set; }
    }
}
