﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FIVES;

namespace StaticSceneryPlugin
{
    [TestFixture()]
    public class StaticSceneryPluginInitializerTest
    {
        StaticSceneryPluginInitializer plugin = new StaticSceneryPluginInitializer();

        // Global instances of FIVES Domain Model
        private ComponentRegistry globalComponentRegistry = ComponentRegistry.Instance;
        private World globalWorld = World.Instance;

        // values to mock configfile
        string SceneryURL = "/static/scenery/url.xml";
        float OffsetX = 1.0f;
        float OffsetY = -1.0f;
        float OffsetZ = 2.0f;

        // the initializer of the test corresponds to the intializier of the plugin. Reading Config is mocked by method, as config file is not
        // read by NUnit
        [SetUp()]
        public void Init()
        {
            World.Instance = new World();
            ComponentRegistry.Instance = new ComponentRegistry();
            MockReadConfig();
            MockComponentRegistry();
            plugin.Initialize();
        }

        /// <summary>
        /// After each test, original World objects of FiVES are restored
        /// </summary>
        [TearDown()]
        public void ShutDown()
        {
            World.Instance = globalWorld;
            ComponentRegistry.Instance = globalComponentRegistry;
        }

        /// <summary>
        /// We need to mock reading the config file here, as NUnit cannot read the external app.config. That's why we simulate setting the
        /// needed values by setting them directly from the test fixture's respective members
        /// </summary>
        void MockReadConfig()
        {
            plugin.SceneryURL = SceneryURL;
            plugin.OffsetX = OffsetX;
            plugin.OffsetY = OffsetY;
            plugin.OffsetZ = OffsetZ;
        }

        /// <summary>
        /// Component Dependencies are not resolved when faced in a test fixture, so we need to manually register the components the plugin needs
        /// to access.
        /// </summary>
        void MockComponentRegistry()
        {
            ComponentDefinition position = new ComponentDefinition("location");
            position.AddAttribute<Vector>("position", new Vector(0, 0, 0));

            ComponentDefinition mesh = new ComponentDefinition("mesh");
            mesh.AddAttribute<string>("uri");

            FIVES.ComponentRegistry.Instance.Register(position);
            FIVES.ComponentRegistry.Instance.Register(mesh);
        }
        /// <summary>
        /// Checks if the entity for Static scenery is present in the World as specified in the Config file
        /// </summary>
        [Test()]
        public void ShouldContainSpecifiedSceneryEntity()
        {
            Assert.IsNotEmpty(FIVES.World.Instance);
            var entity = FIVES.World.Instance.ElementAt(0);

            var pos = (Vector)entity["location"]["position"];
            Assert.AreEqual(pos.x, OffsetX);
            Assert.AreEqual(pos.y, OffsetY);
            Assert.AreEqual(pos.z, OffsetZ);
            Assert.AreEqual(entity["mesh"]["uri"], SceneryURL);
        }
    }
}
