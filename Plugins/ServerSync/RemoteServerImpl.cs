using KIARAPlugin;
using System;

namespace ServerSyncPlugin
{
    class RemoteServerImpl : IRemoteServer
    {
        public RemoteServerImpl(Connection connection, IDomainOfInterest doi, IDomainOfResponsibility dor, Guid syncID)
        {
            this.connection = connection;
            this.doi = doi;
            this.dor = dor;
            this.syncID = syncID;
        }

        public Connection Connection
        {
            get
            {
                return connection;
            }
        }

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


        public event EventHandler DoIChanged;

        public event EventHandler DoRChanged;

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
