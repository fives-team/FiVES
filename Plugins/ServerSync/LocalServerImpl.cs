using KIARAPlugin;
using System;
using System.IO;

namespace ServerSyncPlugin
{
    class LocalServerImpl : ILocalServer
    {
        public LocalServerImpl()
        {
            service = ServiceFactory.Create(ConvertFileNameToURI("serverSyncServer.json"));
            dor = new EmptyDoR();
            doi = new EmptyDoI();
            syncID = Guid.NewGuid();

            RegisterSyncIDAPI();
        }

        public ServiceImpl Service
        {
            get
            {
                return service;
            }
        }

        public IDomainOfResponsibility DoR
        {
            get
            {
                return dor;
            }
            set
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
            set
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

        /// <summary>
        /// Converts a file name to the URI that point to the file as if it was located in the same directory as the
        /// current assembly.
        /// </summary>
        /// <param name="configFilename"></param>
        /// <returns></returns>
        string ConvertFileNameToURI(string configFilename)
        {
            var configFullPath = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), configFilename);
            return "file://" + configFullPath;
        }

        void RegisterSyncIDAPI()
        {
            service["serverSync.getSyncID"] = (Func<Guid>)GetSyncID;
        }

        Guid GetSyncID()
        {
            return syncID;
        }

        ServiceImpl service;
        IDomainOfResponsibility dor;
        IDomainOfInterest doi;
        Guid syncID;
    }
}
