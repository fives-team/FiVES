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
using System.Threading.Tasks;
using FIVES;
using System.Net;
using System.Xml;

namespace RESTServicePlugin
{
    /// <summary>
    /// This Plugin provides basic Scene operation via an HTTP REST Interface. In addition, it exposes
    /// an API to other plugins that allows to extend the rest interface by own services.
    /// </summary>
    public class RESTServicePluginInitializer : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string Name
        {
            get
            {
                return "RESTService";
            }
        }

        public List<string> PluginDependencies
        {
            get { return new List<string> (); }
        }

        public List<string> ComponentDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        public void Initialize()
        {
            XmlDocument configDoc = new XmlDocument();
            configDoc.Load(this.GetType().Assembly.Location + ".config");
            XmlNode restService = configDoc.SelectSingleNode("configuration/restservice");

            RequestListener = new RESTRequestListener(ReadHost(restService), ReadBaseURL(restService));
            RequestListener.Initialize();
            RequestDispatcher.Instance.RegisterHandler(new RESTSceneAPI());
        }

        public void Shutdown()
        {
            RequestListener.Shutdown();
        }

        #endregion

        private string ReadHost(XmlNode restService)
        {
            string host = restService.Attributes["host"].Value;
            string port = restService.Attributes["port"].Value;
            return "http://" + host + ":" + port;
        }

        private string ReadBaseURL(XmlNode restService)
        {
            return GetValidBaseUrl(restService.Attributes["path"].Value);
        }

        private string GetValidBaseUrl(string baseurl)
        {
            if (!baseurl.StartsWith("/"))
                baseurl = String.Concat("/", baseurl);
            if (!baseurl.EndsWith("/"))
                baseurl = String.Concat(baseurl, "/");
            return baseurl;
        }

        private RESTRequestListener RequestListener;
    }
}
