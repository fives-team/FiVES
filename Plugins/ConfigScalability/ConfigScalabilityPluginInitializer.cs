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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;
using ScalabilityPlugin;

namespace ConfigScalabilityPlugin
{
    public class ConfigScalabilityPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get
            {
                return "ConfigScalability";
            }
        }

        public List<string> PluginDependencies
        {
            get
            {
                return new List<string> { "ServerSync", "Scalability" };
            }
        }

        public List<string> ComponentDependencies
        {
            get
            {
                return new List<string> { "location" };
            }
        }

        public void Initialize()
        {
            // Registers ConfigScalability as the class implementing IScalability interface.
            Scalability.Instance = new ConfigScalability();
        }

        public void Shutdown()
        {
        }
    }
}
