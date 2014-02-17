using System;
using NUnit.Framework;
using FIVES;

namespace LocationPlugin
{
    [TestFixture()]
    public class LocationPluginInitializerTest
    {
        LocationPluginInitializer plugin = new LocationPluginInitializer();

        ComponentRegistry globalComponentRegistry = ComponentRegistry.Instance;

        [SetUp()]
        public void Init()
        {
            ComponentRegistry.Instance = new ComponentRegistry();
            plugin.Initialize();
        }

        [TearDown()]
        public void ShutDown()
        {
            ComponentRegistry.Instance = globalComponentRegistry;
        }

        [Test()]
        public void ShouldRegisterLocationComponent()
        {
            Assert.IsNotNull(ComponentRegistry.Instance.FindComponentDefinition("location"));
        }
    }
}

