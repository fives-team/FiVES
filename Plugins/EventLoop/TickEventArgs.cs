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
using System.Threading.Tasks;

namespace EventLoopPlugin
{
    /// <summary>
    /// EventArgs that are used by the TickFired-Event of the eventloop provided by the EventLoop-Plugin. The EventArgs contain the time at which the
    /// event was fired relative to the start of the server.
    /// </summary>
    public class TickEventArgs : EventArgs
    {
        public TickEventArgs(TimeSpan timestamp)
        {
            TimeStamp = timestamp;
        }
        public TimeSpan TimeStamp { get; private set; }
    }
}
