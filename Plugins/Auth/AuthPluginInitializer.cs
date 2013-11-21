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

        public List<string> PluginDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        public List<string> ComponentDependencies
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

