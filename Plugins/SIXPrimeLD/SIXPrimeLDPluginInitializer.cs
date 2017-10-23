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
using SIXPrime.Server;
using System;
using System.Collections.Generic;

namespace SIXPrimeLDPlugin
{
    public class SIXPrimeLDPluginInitializer : IPluginInitializer
    {
        public List<string> ComponentDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        public string Name
        {
            get
            {
                return "SIXPrimeLD";
            }
        }

        public List<string> PluginDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        public void Initialize()
        {
            // TO SEE USAGE OF SIX'LD, AND SIX'LD IN ACTION, UNCOMMENT THE FOLLOWING LINE
            // Demo.startDemo(server, worldUri);
        }

        public void Shutdown()
        {
            Console.WriteLine("[SIXPrimeLD] shutdown");
        }

        private static Uri baseUri = new Uri("http://localhost:12345/");
        private static Uri worldUri = new Uri(baseUri.OriginalString + "world");
        private static Server server = new Server();
    }
}
