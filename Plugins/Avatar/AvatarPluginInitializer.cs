using System;
using FIVES;
using System.Collections.Generic;
using AuthPlugin;
using ClientManagerPlugin;
using KIARAPlugin;

namespace AvatarPlugin
{
    public class AvatarPluginInitializer : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string Name
        {
            get
            {
                return "Avatar";
            }
        }

        public List<string> PluginDependencies
        {
            get
            {
                return new List<string> { "ClientManager", "Auth", "KIARA" };
            }
        }

        public List<string> ComponentDependencies
        {
            get
            {
                return new List<string> { "meshResource", "scale", "velocity", "rotVelocity" };
            }
        }

        public void Initialize ()
        {
            ComponentDefinition avatar = new ComponentDefinition("avatar");
            avatar.AddAttribute<string>("userLogin", null);
            ComponentRegistry.Instance.Register(avatar);

            ClientManager.Instance.RegisterClientService("avatar", true, new Dictionary<string, Delegate> {
                {"getAvatarEntityGuid", (Func<Connection, string>)GetAvatarEntityGuid},
                {"changeAppearance", (Action<Connection, string, Vector>)ChangeAppearance},
                {"startAvatarMotionInDirection", (Action<Connection, Vector>)StartAvatarMotionInDirection},
                {"setAvatarForwardBackwardMotion", (Action<Connection, float>)SetForwardBackwardMotion},
                {"setAvatarLeftRightMotion", (Action<Connection, float>)SetLeftRightMotion},
                {"setAvatarSpinAroundAxis",(Action<Connection, Vector, float>)SetAvatarSpinAroundAxis}
            });

            ClientManager.Instance.NotifyWhenAnyClientAuthenticated(delegate(Connection connection) {
                Activate(connection);
                connection.Closed += (sender, e) => Deactivate(connection);
            });

            World.Instance.AddedEntity += HandleAddedEntity;

            foreach (var entity in World.Instance)
                CheckAndRegisterAvatarEntity(entity);
        }

        public void Shutdown()
        {
        }

        void HandleAddedEntity (object sender, EntityEventArgs e)
        {
            if (!CheckAndRegisterAvatarEntity(e.Entity))
                e.Entity.CreatedComponent += HandleCreatedComponent;
        }

        void HandleCreatedComponent(object sender, ComponentEventArgs e)
        {
            if (e.Component.Name == "avatar")
                e.Component.ChangedAttribute += HandleChangedAvatarComponent;
        }

        void HandleChangedAvatarComponent(object sender, ChangedAttributeEventArgs e)
        {
            if (e.AttributeName == "userLogin")
            {
                if (e.OldValue != null && avatarEntities.ContainsKey((string)e.OldValue))
                    avatarEntities.Remove((string)e.OldValue);
                avatarEntities[(string)e.NewValue] = e.Entity;
            }
        }

        bool CheckAndRegisterAvatarEntity(Entity entity)
        {
            if (entity.ContainsComponent("avatar"))
            {
                avatarEntities[(string)entity["avatar"]["userLogin"]] = entity;
                return true;
            }

            return false;
        }

        #endregion

        Entity GetAvatarEntityByConnection(Connection connection)
        {
            var userLogin = Authentication.Instance.GetLoginName(connection);
            if (!avatarEntities.ContainsKey(userLogin)) {
                Entity newAvatar = new Entity();
                newAvatar["avatar"]["userLogin"] = userLogin;
                newAvatar["meshResource"]["uri"] = defaultAvatarMesh;
                newAvatar["meshResource"]["visible"] = true;
                World.Instance.Add(newAvatar);
            }

            return avatarEntities[userLogin];
        }

		/// <summary>
        /// Kiara service interface function to let a connected client query the guid of the entity that was created for the avatar
        /// </summary>
        /// <param name="connection">Client connection</param>
        /// <returns>The Guid of the Entity used as avatar</returns>
        private string GetAvatarEntityGuid(Connection connection)
        {
            Entity avatarEntity = GetAvatarEntityByConnection(connection);
            return avatarEntity.Guid.ToString();
        }

        /// <summary>
        /// Activates the avatar entity. Can also be used to update the mesh when its changed.
        /// </summary>
        /// <param name="connection">Client connection</param>
        void Activate(Connection connection)
        {
            var avatarEntity = GetAvatarEntityByConnection(connection);
            avatarEntity["meshResource"]["visible"] = true;
        }

        /// <summary>
        /// Deactivates the avatar entity.
        /// </summary>
        /// <param name="connection">Client connection</param>
        void Deactivate(Connection connection)
        {
            var avatarEntity = GetAvatarEntityByConnection(connection);
            avatarEntity["meshResource"]["visible"] = false;
        }

        /// <summary>
        /// Changes the appearance of the avatar.
        /// </summary>
        /// <param name="connection">Client connection</param>
        /// <param name="meshURI">New mesh URI.</param>
        /// <param name="scale">New scale.</param>
        void ChangeAppearance(Connection connection, string meshURI, Vector scale)
        {
            var avatarEntity = GetAvatarEntityByConnection(connection);

            avatarEntity["meshResource"]["uri"] = meshURI;

            avatarEntity["scale"]["x"] = scale.x;
            avatarEntity["scale"]["y"] = scale.y;
            avatarEntity["scale"]["z"] = scale.z;
        }

        void StartAvatarMotionInDirection(Connection connection, Vector velocity)
        {
            var avatarEntity = GetAvatarEntityByConnection(connection);

            avatarEntity["velocity"]["x"] = (float)velocity.x;
            avatarEntity["velocity"]["y"] = (float)velocity.y;
            avatarEntity["velocity"]["z"] = (float)velocity.z;
        }

        void SetForwardBackwardMotion(Connection connection, float amount)
        {
            var avatarEntity = GetAvatarEntityByConnection(connection);
            avatarEntity["velocity"]["x"] = amount;
        }

        void SetLeftRightMotion(Connection connection, float amount)
        {
            var avatarEntity = GetAvatarEntityByConnection(connection);
            avatarEntity["velocity"]["z"] = amount;
        }

        void SetAvatarSpinAroundAxis(Connection connection, Vector axis, float angle)
        {
            var avatarEntity = GetAvatarEntityByConnection(connection);
            avatarEntity["rotVelocity"]["x"] = axis.x;
            avatarEntity["rotVelocity"]["y"] = axis.y;
            avatarEntity["rotVelocity"]["z"] = axis.z;
            avatarEntity["rotVelocity"]["r"] = angle;
        }
		
        Dictionary<string, Entity> avatarEntities = new Dictionary<string, Entity>();
        // string defaultAvatarMesh = "resources/models/defaultAvatar/avatar.xml3d";
        string defaultAvatarMesh = "resources/proprietary/natalieFives/xml3d/natalie.xml";
    }
}
