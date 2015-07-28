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
using SINFONI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("PluginManager")] // FOR SETTING KTD IN PLUGIN MANAGER
[assembly: InternalsVisibleTo("ServerSync")] // FOR TESTING
namespace FIVES
{
    /// <summary>
    /// Represents a collection of top-level entities in the world.
    /// </summary>
    public sealed class World : EntityCollection
    {
        public Guid ID { get; private set; }
        public SinTD SinTd { get; internal set; }

        internal World()
        {
            ID = Guid.NewGuid();
        }

        public static World Instance = new World();
    }
}
