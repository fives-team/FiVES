using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;
using NUnit.Framework;


namespace AvatarCollisionPlugin
{
    [TestFixture()]
    class AvatarCollisionTest
    {
        [SetUp()]
        public void SetUpTest()
        {
            ComponentRegistry.Instance = new ComponentRegistry();
            var p = new AvatarCollisionPluginInitializer();
            p.RegisterComponents();
            p.RegisterToEvents();
            registerLocationComponent();
            registerAvatarComponent();
        }

        /// <summary>
        /// Mocks registration of location component as dependency to location is not resolved by
        /// PluginManager
        /// </summary>
        private void registerLocationComponent()
        {
            ComponentDefinition location = new ComponentDefinition("location");
            location.AddAttribute<Vector>("position", new Vector(0, 0, 0));
            location.AddAttribute<Quat>("orientation", new Quat(0, 0, 0, 1));
            ComponentRegistry.Instance.Register(location);
        }

        /// <summary>
        /// Mocks registration of avatar component as dependency to location is not resolved by
        /// PluginManager and check for existance of avatar component is performed before
        /// collision operations are performed
        /// </summary>
        private void registerAvatarComponent()
        {
            ComponentDefinition avatar = new ComponentDefinition("avatar");
            avatar.AddAttribute<string>("userLogin", null);
            ComponentRegistry.Instance.Register(avatar);
        }

        /// <summary>
        /// Tests if entities that are added to the world are initialized with groundlevel equal to
        /// their actual height
        /// </summary>
        [Test()]
        public void ShouldInitializeGroundlevelCorrectly()
        {
            var e = new Entity();
            e["avatar"]["userLogin"] = "123";
            e["location"]["position"] = new Vector(0, 5, 0);
            World.Instance.Add(e);
            Assert.AreEqual((float)e["avatarCollision"]["groundLevel"], 5f);
        }

        /// <summary>
        /// Tests if a change in an entity's location correctly sets the entity's height to the
        /// groundlevel stored for this entity
        /// </summary>
        [Test()]
        public void ShouldSetEntityToGroundlevel()
        {
            var e = new Entity();
            e["avatar"]["userLogin"] = "123";
            e["avatarCollision"]["groundLevel"] = 0f;
            World.Instance.Add(e);
            e["location"]["position"] = new Vector(1, 2, 3);
            Vector entityPosition = (Vector)e["location"]["position"];
            Assert.AreEqual(entityPosition.x, 1f);
            Assert.AreEqual(entityPosition.y, 0f);
            Assert.AreEqual(entityPosition.z, 3f);
        }
    }
}
