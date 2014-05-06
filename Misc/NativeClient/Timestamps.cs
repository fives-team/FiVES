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

namespace NativeClient
{
    class Timestamps
    {
        /// <summary>
        /// Computes Unix timestamp for the current moment.
        /// </summary>
        /// <value>The unix timestamp.</value>
        public static long UnixTimestamp
        {
            get
            {
                TimeSpan span = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
                return (long)span.TotalSeconds;
            }
        }

        /// <summary>
        /// Returns a number of milliseconds that has passed since start of the day. This value will be relatively
        /// small and may be precisely stored in a float.
        /// </summary>
        /// <value>Number of milliseconds since start of the day.</value>
        public static float FloatMilliseconds
        {
            get
            {
                TimeSpan span = (DateTime.Now - DateTime.Today);
                return (float)span.TotalMilliseconds;
            }
        }
    }
}

