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
        public void ShouldRegisterPositionComponent()
        {
            Assert.IsNotNull(ComponentRegistry.Instance.FindComponentDefinition("meshResource"));
        }

        [Test()]
        public void ShouldRegisterOrientationComponent()
        {
            Assert.IsNotNull(ComponentRegistry.Instance.FindComponentDefinition("scale"));
        }
    }
}

