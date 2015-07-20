// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using FIVES;
using System.Collections.Generic;
using ClientManagerPlugin;
using System.IO;
using KIARAPlugin;
using SINFONI;

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
                return new List<string> {"ClientManager"};
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
            RegisterService();
        }

        public void Shutdown()
        {
        }

        #endregion

        private void RegisterService()
        {
            string authIDL = File.ReadAllText("auth.kiara");
            SINFONIServerManager.Instance.KiaraServer.AmendIDL(authIDL);
            ClientManager.Instance.RegisterClientService("auth", false, new Dictionary<string, Delegate> {
                {"login", (Func<Connection, string, string, bool>)Authentication.Instance.Authenticate}
            });
        }
    }
}

