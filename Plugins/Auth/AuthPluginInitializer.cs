using System;
using FIVES;
using System.Collections.Generic;

namespace AuthPlugin
{
    public class AuthPluginInitializer : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string Name
        {
            get
            {
                return "Auth";
            }
        }

        public List<string> RequiredPlugins
        {
            get
            {
                return new List<string>();
            }
        }

        public List<string> RequiredComponents
        {
            get
            {
                return new List<string>();
            }
        }

        public void Initialize ()
        {
        }

        #endregion
    }
}

