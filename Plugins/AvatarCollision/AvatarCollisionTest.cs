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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;
using FIVESServiceBus;
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

            ServiceBus.Instance = new ServiceBusImplementation();
            ServiceBus.Instance.Initialize();

            ServiceBus.ServiceRegistry.RegisterService("collision", p.Transform);
            ServiceBus.Instance.IntroduceTopic("location.position", "collision");
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
            e["avatar"]["userLogin"].Suggest("123");
            e["location"]["position"].Suggest(new Vector(0, 5, 0));

            World.Instance.Add(e);

            Assert.AreEqual((float)e["avatarCollision"]["groundLevel"].Value, 5f);
        }

        /// <summary>
        /// Tests if a change in an entity's location correctly sets the entity's height to the
        /// groundlevel stored for this entity
        /// </summary>
        [Test()]
        public void ShouldSetEntityToGroundlevel()
        {
            var e = new Entity();
            e["avatar"]["userLogin"].Suggest("123");
            e["avatarCollision"]["groundLevel"].Suggest(0f);
            World.Instance.Add(e);
            e["location"]["position"].Suggest(new Vector(1, 2, 3));

            bool finishedComputation = false;
            Vector entityPosition = new Vector(0,0,0);
            ServiceBus.Instance.ComputedResult += (o, r) =>
                {
                    if (r.AccumulatedTransformations.ContainsKey("location"))
                    {
                        finishedComputation = true;
                        entityPosition = (Vector)e["location"]["position"].Value;
                        Assert.AreEqual(entityPosition.x, 1f);
                        Assert.AreEqual(entityPosition.y, 0f);
                        Assert.AreEqual(entityPosition.z, 3f);
                    }
                };

            var delay = 25;
            var pollingInterval = 100; // 100 milliseconds

            // Check that the computation has actually finished correctly
            Assert.That(() => finishedComputation, Is.EqualTo(true).After(delay, pollingInterval));
        }
    }
}
