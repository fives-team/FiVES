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
using System.Net;
using System.Text;

namespace ScalabilityPlugin
{
    /// <summary>
    /// Interface that a concrete scalability class should implement.
    /// </summary>
    public interface IScalability
    {
        /// <summary>
        /// Searches for the server responsible for a given entity and returns its IP address.
        /// </summary>
        /// <param name="entity">A given entity.</param>
        /// <returns>Responsible server's IP address.</returns>
        IPAddress FindResponsibleServer(Entity entity);
    }
}
