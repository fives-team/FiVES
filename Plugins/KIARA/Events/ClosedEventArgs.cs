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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KIARAPlugin
{
    /// <summary>
    /// Event arguments for Close event in Connection class.
    /// </summary>
    public class ClosedEventArgs : EventArgs
    {
        public ClosedEventArgs(string reason)
        {
            Reason = reason;
        }

        /// <summary>
        /// Reason for closing the connection.
        /// </summary>
        public string Reason { get; private set; }
    }
}
