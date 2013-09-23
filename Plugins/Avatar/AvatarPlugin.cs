using System;
using FIVES;
using System.Collections.Generic;
using KIARA;
using Location;

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
            return new List<string> { "ClientManager", "Auth", "Renderable", "Location" };
        }

        public void Initialize ()
        {
            ComponentLayout avatarLayout = new ComponentLayout();
            avatarLayout.AddAttribute<string>("userLogin", null);
            ComponentRegistry.Instance.DefineComponent("avatar", pluginGuid, avatarLayout);

            var authService = ServiceFactory.DiscoverByName("auth", ContextFactory.GetContext("inter-plugin"));
            authService.OnConnected += (conn) => authPlugin = conn;

            var clientManager =
                ServiceFactory.DiscoverByName("clientmanager", ContextFactory.GetContext("inter-plugin"));
            clientManager.OnConnected += delegate(Connection connection) {
                connection["registerClientService"]("avatar", true, new Dictionary<string, Delegate> {
                    {"changeAppearance", (Action<string, string, Vector>)ChangeAppearance},
                });

                connection["notifyWhenAnyClientAuthenticated"]((Action<Guid>)delegate(Guid sessionKey) {
                    Activate(sessionKey);
                    connection["notifyWhenClientDisconnected"](sessionKey, (Action<Guid>)Deactivate);
                });
            };

            foreach (var guid in EntityRegistry.Instance.GetAllGUIDs()) {
                var entity = EntityRegistry.Instance.GetEntity(guid);
                if (entity.HasComponent("avatar"))
                    avatarEntities[(string)entity["avatar"]["userLogin"]] = entity;
            }
        }

        #endregion

        Entity GetAvatarEntityBySessionKey(Guid sessionKey)
        {
            var userLogin = authPlugin["getLoginName"](sessionKey).Wait<string>();
            if (!avatarEntities.ContainsKey(userLogin)) {
                Entity newAvatar = new Entity();
                newAvatar["avatar"]["userLogin"] = userLogin;
                newAvatar["meshResource"]["uri"] = defaultAvatarMesh;
                newAvatar["meshResource"]["visible"] = false;
                EntityRegistry.Instance.AddEntity(newAvatar);
                avatarEntities[userLogin] = newAvatar;
            }

            return avatarEntities[userLogin];
        }

        /// <summary>
        /// Activates the avatar entity. Can also be used to update the mesh when its changed.
        /// </summary>
        /// <param name="sessionKey">Client session key.</param>
        void Activate(Guid sessionKey)
        {
            var avatarEntity = GetAvatarEntityBySessionKey(sessionKey);
            avatarEntity["meshResource"]["visible"] = true;
        }

        /// <summary>
        /// Deactivates the avatar entity.
        /// </summary>
        /// <param name="sessionKey">Client session key.</param>
        void Deactivate(Guid sessionKey)
        {
            var avatarEntity = GetAvatarEntityBySessionKey(sessionKey);
            avatarEntity["meshResource"]["visible"] = false;
        }

        /// <summary>
        /// Changes the appearance of the avatar.
        /// </summary>
        /// <param name="sessionKey">Client session key.</param>
        /// <param name="meshURI">New mesh URI.</param>
        /// <param name="scale">New scale.</param>
        void ChangeAppearance(string sessionKey, string meshURI, Vector scale)
        {
            var avatarEntity = GetAvatarEntityBySessionKey(Guid.Parse(sessionKey));

            avatarEntity["meshResource"]["uri"] = meshURI;

            avatarEntity["scale"]["x"] = scale.x;
            avatarEntity["scale"]["y"] = scale.y;
            avatarEntity["scale"]["z"] = scale.z;
        }

        Dictionary<string, Entity> avatarEntities = new Dictionary<string, Entity>();
        // string defaultAvatarMesh = "resources/models/defaultAvatar/avatar.xml3d";
        string defaultAvatarMesh = "resources/models/firetruck/xml3d/firetruck.xml";
        Guid pluginGuid = new Guid("54b1215e-22cc-44ed-bef4-c92e4fb4edb5");
        Connection authPlugin;
    }
}

