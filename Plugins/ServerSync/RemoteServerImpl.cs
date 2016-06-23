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
    /// Implementation of the IRemoteServer interface.
    /// </summary>
    class RemoteServerImpl : IRemoteServer
    {
        /// <summary>
        /// Constructs a remote server object.
        /// </summary>
        /// <param name="connection">An established connection to the remote server.</param>
        /// <param name="doi">Remote server's DoI.</param>
        /// <param name="dor">Remote server's DoR.</param>
        /// <param name="syncID">Remote server's SyncID.</param>
        public RemoteServerImpl(Connection connection, IDomainOfInterest doi, IDomainOfResponsibility dor, Guid syncID)
        {
            this.connection = connection;
            this.doi = doi;
            this.dor = dor;
            this.syncID = syncID;
        }

        /// <summary>
        /// SINFONI connection to the remote server.
        /// </summary>
        public Connection Connection
        {
            get
            {
                return connection;
            }
        }

        /// <summary>
        /// Remote domain-of-reponsibility.
        /// </summary>
        public IDomainOfResponsibility DoR
        {
            get
            {
                return dor;
            }
            internal set
            {
                dor = value;
                if (DoRChanged != null)
                    DoRChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// Remote domain-of-interest.
        /// </summary>
        public IDomainOfInterest DoI
        {
            get
            {
                return doi;
            }
            internal set
            {
                doi = value;
                if (DoIChanged != null)
                    DoIChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// Triggered when the remote DoI has changed.
        /// </summary>
        public event EventHandler DoIChanged;

        /// <summary>
        /// Triggered when the remote DoR has changed.
        /// </summary>
        public event EventHandler DoRChanged;

        /// <summary>
        /// Remote SyncID.
        /// </summary>
        public Guid SyncID
        {
            get
            {
                return syncID;
            }
        }

        IDomainOfResponsibility dor;
        IDomainOfInterest doi;
        Connection connection;
        Guid syncID;
    }
}
