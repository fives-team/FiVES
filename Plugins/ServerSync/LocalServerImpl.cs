using KIARAPlugin;
using System;
using System.IO;
using Newtonsoft.Json;

namespace ServerSyncPlugin
{
    class LocalServerImpl : ILocalServer
    {
        public LocalServerImpl()
        {
            dor = new EmptyDoR();
            doi = new EmptyDoI();
            syncID = Guid.NewGuid();

            service = ServiceFactory.Create(ServerSyncTools.ConvertFileNameToURI("serverSyncServer.json"));
            service.OnNewClient += ServerSyncTools.ConfigureJsonSerializer;

            RegisterSyncIDAPI(service);
        }

        public IServiceImpl Service
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
                if (value == null)
                    throw new ArgumentNullException("DoR can not be set to null.");

                dor.Changed -= HandleDoRChanged;
                dor = value;
                dor.Changed += HandleDoRChanged;

                HandleDoRChanged(this, new EventArgs());
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
                if (value == null)
                    throw new ArgumentNullException("DoI can not be set to null.");

                doi.Changed -= HandleDoIChanged;
                doi = value;
                doi.Changed += HandleDoIChanged;

                HandleDoIChanged(this, new EventArgs());
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

        public void RegisterSyncIDAPI(IService service)
        {
            service["serverSync.getSyncID"] = (Func<Guid>)GetSyncID;
        }

        void HandleDoIChanged(object sender, EventArgs e)
        {
            if (DoIChanged != null)
                DoIChanged(this, new EventArgs());
        }

        void HandleDoRChanged(object sender, EventArgs e)
        {
            if (DoRChanged != null)
                DoRChanged(this, new EventArgs());
        }

        Guid GetSyncID()
        {
            return syncID;
        }

        IServiceImpl service;
        IDomainOfResponsibility dor;
        IDomainOfInterest doi;
        Guid syncID;
    }
}
