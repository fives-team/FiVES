using NUnit.Framework;
using System;
using FIVES;

namespace RenderablePlugin
{
    [TestFixture()]
    public class RenderablePluginInitializerTest
    {
        RenderablePluginInitializer plugin = new RenderablePluginInitializer();

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
        public void ShouldRegisterMeshComponent()
        {
            Assert.IsNotNull(ComponentRegistry.Instance.FindComponentDefinition("mesh"));
        }
    }
}

