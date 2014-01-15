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
                return new List<string> { "KIARA" };
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
            Authentication.Instance = new Authentication();
        }

        public void Shutdown()
        {
        }

        #endregion
    }
}

