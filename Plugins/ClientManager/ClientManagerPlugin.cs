using System;
using System.Collections.Generic;
using FIVES;


namespace ClientManagerPlugin {

    /// <summary>
    /// Implements a plugin that can be used to communicate with clients using KIARA.
    /// </summary>
    public class ClientManagerPluginInitializer : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string GetName()
        {
            return "ClientManager";
        }

        public List<string> GetDependencies()
        {
            return new List<string>() { "Auth" };
        }

        public void Initialize()
        {
        }

        #endregion
    }

}