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
        public Guid Authenticate(string login, string password)
        {
            // Currently we just accept any login/password combinations.
            var securityToken = Guid.NewGuid();
            guidToLogin[securityToken] = login;
            return securityToken;
        }

        /// <summary>
        /// Returns login name for a given <paramref name="securityToken"/>.
        /// </summary>
        /// <returns>The login name or null if such a token is not defined.</returns>
        /// <param name="securityToken">The security token.</param>
        public string GetLoginName(Guid securityToken)
        {
            if (guidToLogin.ContainsKey(securityToken))
                return guidToLogin[securityToken];
            return null;
        }

        private Dictionary<Guid, string> guidToLogin = new Dictionary<Guid, string>();
    }
}
