using System;
using FIVES;
using System.Collections.Generic;
using KIARA;
using ClientManager;

namespace Avatar
{
    public class AvatarPlugin : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string GetName ()
        {
            return "Avatar";
        }

        public List<string> GetDependencies ()
        {
            return new List<string> { "ClientManager", "Auth", "DirectCall" };
        }

        public void Initialize ()
        {
            var clientManager = ServiceFactory.DiscoverByName("clientmanager", ContextFactory.GetContext("inter-plugin"));
            clientManager.OnConnected += delegate(Connection connection) {
                connection["registerClientService"]("avatar", new Dictionary<string, Delegate> {
                    {"create", (Func<string, string>)CreateAvatar},
                    {"create", (Func<string, Vector, Quat, Vector, string>)CreateAvatar}
                });
            };
        }

        #endregion

        internal string CreateAvatar(string meshURI)
        {
            throw new NotImplementedException();
        }

        internal string CreateAvatar(string meshURI, Vector position, Quat orientation, Vector scale)
        {
            throw new NotImplementedException();
        }
    }
}

