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
using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ValidPlugin3
{
    public class ValidPlugin3Initializer : IPluginInitializer
    {
        public string Name
        {
            get 
            {
                return "ValidPlugin3";
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
                return new List<string> { "testComponentForValidPlugin3" };
            }
        }

        public void Initialize()
        {
        }

        public void Shutdown()
        {
        }
    }
}
