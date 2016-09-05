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
using SINFONI;
using System;

namespace ServerSyncPlugin
{
    /// <summary>
    /// Represents a remote server.
    /// </summary>
    public interface IRemoteServer
    {
        /// <summary>
        /// SINFONI connection to the remote server.
        /// </summary>
        Connection Connection { get; }

        /// <summary>
        /// Remote domain-of-reponsibility.
        /// </summary>
        IDomainOfResponsibility DoR { get; }

        /// <summary>
        /// Remote domain-of-interest.
        /// </summary>
        IDomainOfInterest DoI { get; }

        /// <summary>
        /// Remote SyncID.
        /// </summary>
        Guid SyncID { get; }

        /// <summary>
        /// Triggered when the remote DoI has changed.
        /// </summary>
        event EventHandler DoIChanged;

        /// <summary>
        /// Triggered when the remote DoR has changed.
        /// </summary>
        event EventHandler DoRChanged;
    }
}
