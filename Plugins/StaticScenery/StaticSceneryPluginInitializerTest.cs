using System;
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

        // values to mock configfile
        string SceneryURL = "/static/scenery/url.xml";
        float OffsetX = 1.0f;
        float OffsetY = -1.0f;
        float OffsetZ = 2.0f;

        // the initializer of the test corresponds to the intializier of the plugin. Reading Config is mocked by method, as config file is not
        // read by NUnit
        public StaticSceneryPluginInitializerTest()
        {
            MockReadConfig();
            MockComponentRegistry();
            plugin.Initialize();
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
            ComponentDefinition position = new ComponentDefinition("position");
            position.AddAttribute<float>("x");
            position.AddAttribute<float>("y");
            position.AddAttribute<float>("z");

            ComponentDefinition meshResource = new ComponentDefinition("meshResource");
            meshResource.AddAttribute<string>("uri");

            FIVES.ComponentRegistry.Instance.Register(position);
            FIVES.ComponentRegistry.Instance.Register(meshResource);
        }
        /// <summary>
        /// Checks if the entity for Static scenery is present in the World as specified in the Config file
        /// </summary>
        [Test()]
        public void ShouldContainSpecifiedSceneryEntity()
        {
            Assert.IsNotEmpty(FIVES.World.Instance);
            var entity = FIVES.World.Instance.ElementAt(0);
            Assert.AreEqual(entity["position"]["x"], OffsetX);
            Assert.AreEqual(entity["position"]["y"], OffsetY);
            Assert.AreEqual(entity["position"]["z"], OffsetZ);
            Assert.AreEqual(entity["meshResource"]["uri"], SceneryURL);
        }
    }
}
