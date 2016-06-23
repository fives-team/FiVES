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
using System.Configuration;
using System.Linq;
using System.Text;

namespace ServerSyncPlugin
{
    public class RemoteServersSection : ConfigurationSection
    {
        [ConfigurationProperty("servers")]
        [ConfigurationCollection(typeof(RemoteServerElement), AddItemName="server")]
        public RemoteServerCollection Servers
        {
            get { return (RemoteServerCollection)this["servers"]; }
        }
    }

    public class RemoteServerElement : ConfigurationElement
    {
        [ConfigurationProperty("url", IsRequired=true, IsKey=true)]
        public string Url
        {
            get { return (string)this["url"]; }
        }
    }

    public class RemoteServerCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new RemoteServerElement();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RemoteServerElement)element).Url;
        }
    }
}
