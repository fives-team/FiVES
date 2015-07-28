// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using FIVES;
using System.Collections.Generic;
using AuthPlugin;
using ClientManagerPlugin;
using SINFONI;
using System.Net;
using System.Reflection;
using System.IO;
using System.Xml;
using SINFONIPlugin;

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
                return new List<string> { "SINFONI", "ClientManager", "Auth"};
            }
        }

        public List<string> ComponentDependencies
        {
            get
            {
                return new List<string> { "mesh", "motion" };
            }
        }

        public void Initialize ()
        {
            RegisterComponent();
            ReadConfig();
            RegisterEvents();
            RegisterSinfoniService();
        }

        public void Shutdown()
        {
        }

        void RegisterComponent()
        {
            ComponentDefinition avatar = new ComponentDefinition("avatar");
            avatar.AddAttribute<string>("userLogin", null);
            ComponentRegistry.Instance.Register(avatar);
        }

        void ReadConfig()
        {
            try
            {
                XmlDocument configDocument = new XmlDocument();
                configDocument.Load(this.GetType().Assembly.Location + ".config");
                defaultAvatarMesh = configDocument.SelectSingleNode("configuration/defaultMesh").Attributes["value"].Value;
            }
            catch(Exception e)
            {
                Console.WriteLine("[AvatarPlugin] An Error occurred while processing config file : " + e.Message);
            }
        }

        void RegisterSinfoniService()
        {
            AmendSinfoniServiceIdl();
            ClientManager.Instance.RegisterClientService("avatar", true, new Dictionary<string, Delegate> {
                {"getAvatarEntityGuid", (Func<Connection, string>)GetAvatarEntityGuid},
                {"changeAppearance", (Action<Connection, string, Vector>)ChangeAppearance},
                {"startAvatarMotionInDirection", (Action<Connection, Vector>)StartAvatarMotionInDirection},
                {"setAvatarForwardBackwardMotion", (Action<Connection, float>)SetForwardBackwardMotion},
                {"setAvatarLeftRightMotion", (Action<Connection, float>)SetLeftRightMotion},
                {"setAvatarSpinAroundAxis",(Action<Connection, Vector, float>)SetAvatarSpinAroundAxis}
            });
        }

        void AmendSinfoniServiceIdl()
        {
            var idlContent = File.ReadAllText("avatar.kiara");
            SINFONIServerManager.Instance.SinfoniServer.AmendIDL(idlContent);
        }

        void RegisterEvents()
        {
            ClientManager.Instance.NotifyWhenAnyClientAuthenticated(delegate(Connection connection)
            {
                Activate(connection);
                connection.Closed += (sender, e) => Deactivate(connection);
            });

            World.Instance.AddedEntity += HandleAddedEntity;

            foreach (var entity in World.Instance)
                CheckAndRegisterAvatarEntity(entity);
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
                avatarEntities[(string)entity["avatar"]["userLogin"].Value] = entity;
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
                newAvatar["avatar"]["userLogin"].Suggest(userLogin);
                newAvatar["mesh"]["uri"].Suggest(defaultAvatarMesh);
                newAvatar["mesh"]["visible"].Suggest(true);
                newAvatar["location"]["position"].Suggest(new Vector(0, 10, 0));
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
            avatarEntity["mesh"]["visible"].Suggest(true);
        }

        /// <summary>
        /// Deactivates the avatar entity.
        /// </summary>
        /// <param name="connection">Client connection</param>
        void Deactivate(Connection connection)
        {
            var avatarEntity = GetAvatarEntityByConnection(connection);
            avatarEntity["mesh"]["visible"].Suggest(false);
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

            avatarEntity["mesh"]["uri"].Suggest(meshURI);
            avatarEntity["mesh"]["scale"].Suggest(scale);
        }

        void StartAvatarMotionInDirection(Connection connection, Vector velocity)
        {
            var avatarEntity = GetAvatarEntityByConnection(connection);

            avatarEntity["motion"]["velocity"].Suggest(velocity);
        }

        void SetForwardBackwardMotion(Connection connection, float amount)
        {
            var avatarEntity = GetAvatarEntityByConnection(connection);
            Vector oldVelocity = (Vector)avatarEntity["motion"]["velocity"].Value;
            Vector newVelocity = new Vector(amount, oldVelocity.y, oldVelocity.z);
            avatarEntity["motion"]["velocity"].Suggest(newVelocity); 
        }

        void SetLeftRightMotion(Connection connection, float amount)
        {
            var avatarEntity = GetAvatarEntityByConnection(connection);
            Vector oldVelocity = (Vector)avatarEntity["motion"]["velocity"].Value;
            Vector newVelocity =  new Vector(oldVelocity.x, oldVelocity.y, amount);
            avatarEntity["motion"]["velocity"].Suggest(newVelocity);
        }

        void SetAvatarSpinAroundAxis(Connection connection, Vector axis, float angle)
        {
            var avatarEntity = GetAvatarEntityByConnection(connection);
            avatarEntity["motion"]["rotVelocity"].Suggest(new AxisAngle(axis, angle));
        }

        Dictionary<string, Entity> avatarEntities = new Dictionary<string, Entity>();
        string defaultAvatarMesh = "resources/models/firetruck/xml3d/firetruck.xml";
    }
}
