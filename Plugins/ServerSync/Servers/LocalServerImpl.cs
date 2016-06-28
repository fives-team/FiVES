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
using System.IO;
using Newtonsoft.Json;
using SINFONIPlugin;
using System.Xml;
using System.Configuration;

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
            this.dor = new EmptyDoR();
            this.doi = new EmptyDoI();
            syncID = Guid.NewGuid();

            server = new SINFONIServer(SINFONIServerManager.Instance.ServerURI,
                SINFONIServerManager.Instance.ServerPort,
                "/serversync/",
                "serverSync.sinfoni");

            Configuration serverSyncConfig = ConfigurationManager.OpenExeConfiguration(this.GetType().Assembly.Location);
            int syncPort = int.Parse(serverSyncConfig.AppSettings.Settings["serverSyncPort"].Value);
            service = server.StartService(SINFONIServerManager.Instance.ServiceHost, syncPort, "/", "ws", "fives-json");
            service.OnNewClient += ServerSyncTools.ConfigureJsonSerializer;

            RegisterSyncIDAPI(service);
        }

        public void ShutDown()
        {
            server.ShutDown();
        }
        /// <summary>
        /// SINFONI service on the local service.
        /// </summary>
        public ServiceImplementation Service
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

        public void RegisterSyncIDAPI(Service service)
        {
            service["serverSync.getSyncID"] = (Func<string>)GetSyncID;
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

        string GetSyncID()
        {
            return syncID.ToString();
        }

        SINFONIServer server;
        ServiceImplementation service;
        IDomainOfResponsibility dor;
        IDomainOfInterest doi;
        Guid syncID;
    }
}
