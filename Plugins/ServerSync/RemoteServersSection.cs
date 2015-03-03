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
