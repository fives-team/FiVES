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
using KIARA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AuthPlugin
{
    public class Authentication
    {
        public static Authentication Instance;

        /// <summary>
        /// Authenticate the user with a specified <paramref name="login"/> and <paramref name="password"/>. Returns
        /// the associated security token as GUID. If authentification fails, Guid.Empty is returned.
        /// </summary>
        /// <param name="connection">Client connection.</param>
        /// <param name="login">Login.</param>
        /// <param name="password">Password.</param>
        /// <returns>True on successful authentication, false otherwise.</returns>
        public bool Authenticate(Connection connection, string login, string password)
        {
            // Currently we just accept any login/password combinations.
            connectionToLogin[connection] = login;
            return true;
        }

        /// <summary>
        /// Returns login name for a given connection.
        /// </summary>
        /// <returns>The login name or null if such a connection is not registered.</returns>
        /// <param name="connection">Client connection.</param>
        public string GetLoginName(Connection connection)
        {
            if (connectionToLogin.ContainsKey(connection))
                return connectionToLogin[connection];
            return null;
        }

        private Dictionary<Connection, string> connectionToLogin = new Dictionary<Connection, string>();
    }
}
