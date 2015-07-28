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
using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SINFONIPlugin
{
    public class SINFONIPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get { return "SINFONI"; }
        }

        public List<string> PluginDependencies
        {
            get { return new List<string>(); }
        }

        public List<string> ComponentDependencies
        {
            get { return new List<string>(); }
        }

        public void Initialize()
        {
            SINFONIServerManager.Instance = new SINFONIServerManager();
        }

        public void Shutdown()
        {
            SINFONIServerManager.Instance.SinfoniServer.ShutDown();
        }
    }
}
