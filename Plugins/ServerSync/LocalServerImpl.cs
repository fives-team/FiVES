using KIARAPlugin;
using System;
using System.IO;
using Newtonsoft.Json;

namespace ServerSyncPlugin
{
    /// <summary>
    /// Implementation of the ILocalServer interface.
    /// </summary>
    class LocalServerImpl : ILocalServer
    {
        /// <summary>
        /// Constructs a LocalServerImpl object.
        /// </summary>
        public LocalServerImpl()
        {
            dor = new EmptyDoR();
            doi = new EmptyDoI();
            syncID = Guid.NewGuid();

            service = ServiceFactory.Create(ServerSyncTools.ConvertFileNameToURI("serverSyncServer.json"));
            service.OnNewClient += ServerSyncTools.ConfigureJsonSerializer;

            RegisterSyncIDAPI(service);
        }

        /// <summary>
        /// KIARA service on the local service.
        /// </summary>
        public IServiceImpl Service
        {
            get
            {
                return service;
            }
        }

        /// <summary>
        /// Local domain-of-reponsibility.
        /// </summary>
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

        /// <summary>
        /// Local domain-of-interest.
        /// </summary>
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

        /// <summary>
        /// Triggered when the local DoI has changed.
        /// </summary>
        public event EventHandler DoIChanged;

        /// <summary>
        /// Triggered when the local DoR has changed.
        /// </summary>
        public event EventHandler DoRChanged;

        /// <summary>
        /// Local SyncID.
        /// </summary>
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
