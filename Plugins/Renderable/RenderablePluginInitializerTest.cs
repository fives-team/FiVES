using NUnit.Framework;
using System;
using FIVES;

namespace RenderablePlugin
{
    [TestFixture()]
    public class RenderablePluginInitializerTest
    {
        RenderablePluginInitializer plugin = new RenderablePluginInitializer();

        public RenderablePluginInitializerTest()
        {
            plugin.Initialize();
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

