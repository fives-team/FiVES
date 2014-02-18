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

            service = ServiceFactory.Create(ConversionTools.ConvertFileNameToURI("serverSyncServer.json"));
            service.OnNewClient += ConfigureJsonSerializer;

            RegisterSyncIDAPI(service);
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

        public void RegisterSyncIDAPI(Service service)
        {
            service["serverSync.getSyncID"] = (Func<Guid>)GetSyncID;
        }

        void ConfigureJsonSerializer(Connection connection)
        {
            object settingsObj;
            if (connection.GetProperty("JsonSerializerSettings", out settingsObj))
            {
                JsonSerializerSettings settings = settingsObj as JsonSerializerSettings;
                if (settings != null)
                    settings.TypeNameHandling = TypeNameHandling.Auto;
            }
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
