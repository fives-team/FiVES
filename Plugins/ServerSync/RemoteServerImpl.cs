using KIARAPlugin;
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
        /// KIARA connection to the remote server.
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
