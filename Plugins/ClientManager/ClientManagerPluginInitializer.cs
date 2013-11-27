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

        public string Name
        {
            get
            {
                return "ClientManager";
            }
        }

        public List<string> PluginDependencies
        {
            get
            {
                return new List<string> { "KIARA", "Auth" };
            }
        }

        public List<string> ComponentDependencies
        {
            get
            {
                return new List<string> { "position", "orientation", "scale", "meshResource" };
            }
        }

        public void Initialize()
        {
            ClientManager.Instance = new ClientManager();
        }

        #endregion
    }

}