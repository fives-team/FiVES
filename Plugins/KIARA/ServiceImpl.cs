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

namespace KIARAPlugin
{
    class ServiceImpl : Service, IServiceImpl
    {
        // FIXME: What do we do if we've had new clients before a handler is added? Should we keep the list of all
        // opened connections to invoke a new handler on each of them? What if some of these connection are closed
        // already?
        public event NewClient OnNewClient;

        internal void HandleNewClient(Connection connection)
        {
            RegisterMethods(connection);

            if (OnNewClient != null)
                OnNewClient(connection);
        }

        internal ServiceImpl(Context context) : base(context) {}
    }
}

