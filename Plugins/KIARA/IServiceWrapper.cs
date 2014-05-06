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

namespace KIARAPlugin
{
    public delegate void Connected(Connection connection);

    public interface IServiceWrapper : IService
    {
        /// <summary>
        /// Occurs when connection is established. New handlers are also invoked, even if connection have been
        /// established before they were added.
        /// </summary>
        event Connected OnConnected;
    }
}

