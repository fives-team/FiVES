using System;
using FIVES;
using System.Collections.Generic;
using KIARA;
using ClientSync;

namespace Avatar
{
    public class AvatarPlugin : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string getName ()
        {
            return "Avatar";
        }

        public List<string> getDependencies ()
        {
            return new List<string> { "ClientSync", "Auth" };
        }

        public void initialize ()
        {
            var clientSync = ServiceFactory.discoverByName("clientsync", ContextFactory.getContext("inter-plugin"));
            clientSync.OnConnected += delegate(Connection connection) {
                connection["registerClientService"]("avatar", new Dictionary<string, Delegate> {
                    {"create", (Func<string, string>)createAvatar},
                    {"create", (Func<string, Vector, Quat, Vector, string>)createAvatar}
                });
            };
        }

        #endregion

        internal string createAvatar(string meshURI)
        {
            throw new NotImplementedException();
        }

        internal string createAvatar(string meshURI, Vector position, Quat orientation, Vector scale)
        {
            throw new NotImplementedException();
        }
    }
}

