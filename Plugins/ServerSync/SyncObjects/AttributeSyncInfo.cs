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
    // Optimization: an object of this class is stored per attribute in the World. Therefore, we should carefully
    // choose types to preserve memory. Firstly, there is no need to use 64-bit long for LastTimestamp - a smaller
    // timestamp (e.g. in milliseconds) will suffice. Secondly, 128-bit GUID for LastSyncID is also too large. We can
    // implement an approach where nodes can agree on a small unique identifier that will fit into 16 or even 8 bits.
    // Finally, there is no need to store LastValue in this class as it can be retrieved from the World directly.

    /// <summary>
    /// This class is used represent the sync state of the attribute. It is also used to transfer updates as it
    /// contains all necessary information to consistently decide which value is to be assigned in case of conflicts.
    /// To keep synchronization alrorithm consistent, we've chosen to keep key parts of it in this class, which
    /// includes consistent alrogithm for deciding which value takes precedence and timestamp computation.
    /// </summary>
    class AttributeSyncInfo
    {
        public AttributeSyncInfo() {}

        public AttributeSyncInfo(string lastSyncID, object lastValue)
        {
            // We use number ticks passed since 12:00:00 midnight, January 1, 0001 in UTC timezeon. One tick equals to
            // 100 nanoseconds. This should be precise enough.
            LastTimestamp = DateTime.UtcNow.Ticks;

            LastSyncID = lastSyncID;
            LastValue = lastValue;
        }

        /// <summary>
        /// Timestamp at which the value was assigned. This is used to choose one update over another.
        /// </summary>
        public long LastTimestamp { get; internal set; }

        /// <summary>
        /// SyncID of the sync node which assigned this value. This is used to break ties when timestamps are the same.
        /// </summary>
        public string LastSyncID { get; private set; }

        /// <summary>
        /// This hold the last value of attribute.
        /// </summary>
        public object LastValue { get; private set; }

        /// <summary>
        /// Synchronizes the values of attributes given a remote sync info.
        /// </summary>
        /// <param name="remoteAttrSyncInfo">Remote sync info.</param>
        /// <returns>True if the local attribute sync info has been updated.</returns>
        public bool Sync(AttributeSyncInfo remoteAttrSyncInfo)
        {
            if (remoteAttrSyncInfo.LastTimestamp < LastTimestamp)
                return false;

            // Equality sign in "<=" below is very important, because it ensures that we don't have circulating
            // updates. Should an update return back to the sender it will be discarded as it will have same timestamp
            // and same SyncID.
            if (remoteAttrSyncInfo.LastTimestamp == LastTimestamp &&
                remoteAttrSyncInfo.LastSyncID.CompareTo(LastSyncID) <= 0)
                return false;

            LastValue = remoteAttrSyncInfo.LastValue;
            LastTimestamp = remoteAttrSyncInfo.LastTimestamp;
            LastSyncID = remoteAttrSyncInfo.LastSyncID;

            return true;
        }
    }
}
