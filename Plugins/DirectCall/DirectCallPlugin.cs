using System;
using System.Collections.Generic;
using FIVES;
using KIARA;

namespace DirectCall
{
    public class DirectCallPlugin : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string getName()
        {
            return "DirectCall";
        }

        public List<string> getDependencies()
        {
            return new List<string>();
        }

        public void initialize()
        {
            ProtocolRegistry.Instance.registerProtocolFactory("direct-call", new DCProtocolFactory());
        }

        #endregion
    }
}

