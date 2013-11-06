using System;
using FIVES;
using System.Collections.Generic;

namespace AuthPlugin
{
    public class AuthPluginInitializer : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string GetName ()
        {
            return "Auth";
        }

        public List<string> GetDependencies ()
        {
            return new List<string>();
        }

        public void Initialize ()
        {
        }
        #endregion
    }
}

