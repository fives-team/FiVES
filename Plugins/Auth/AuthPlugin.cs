using System;
using FIVES;
using KIARA;
using System.Collections.Generic;

namespace Auth
{
    public class AuthPlugin : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string getName ()
        {
            return "Auth";
        }

        public System.Collections.Generic.List<string> getDependencies ()
        {
            return new System.Collections.Generic.List<string>();
        }

        public void initialize ()
        {
            var auth = ServiceFactory.createByName("auth", ContextFactory.getContext("inter-plugin"));
            auth["authenticate"] = (Func<string, string, Guid>)authenticate;
            auth["getLoginName"] = (Func<Guid, string>)getLoginName;
        }
        #endregion

        /// <summary>
        /// Authenticate the user with a specified <paramref name="login"/> and <paramref name="password"/>. Returns
        /// the associated security token as GUID. If authentification fails, Guid.Empty is returned.
        /// </summary>
        /// <param name="login">Login.</param>
        /// <param name="password">Password.</param>
        /// <returns>The associated security token.</returns>
        internal Guid authenticate(string login, string password)
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
        internal string getLoginName (Guid securityToken)
        {
            if (guidToLogin.ContainsKey(securityToken))
                return guidToLogin[securityToken];
            return null;
        }

        private Dictionary<Guid, string> guidToLogin = new Dictionary<Guid, string>();
    }
}

