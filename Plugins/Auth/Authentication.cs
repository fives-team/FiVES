using KIARAPlugin;
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
        /// <param name="login">Login.</param>
        /// <param name="password">Password.</param>
        /// <returns>The associated security token.</returns>
        public bool Authenticate(Connection connection, string login, string password)
        {
            // Currently we just accept any login/password combinations.
            connectionToLogin[connection] = login;
            return true;
        }

        /// <summary>
        /// Returns login name for a given connection.
        /// </summary>
        /// <returns>The login name or null if such a token is not defined.</returns>
        /// <param name="securityToken">The security token.</param>
        public string GetLoginName(Connection connection)
        {
            if (connectionToLogin.ContainsKey(connection))
                return connectionToLogin[connection];
            return null;
        }

        private Dictionary<Connection, string> connectionToLogin = new Dictionary<Connection, string>();
    }
}
