using System;
using NUnit.Framework;
using FIVES;

namespace ScriptingPlugin
{
    [TestFixture()]
    public class ScriptingPluginInitializerTest
    {
        ScriptingPluginInitializer plugin = new ScriptingPluginInitializer();

        IComponentRegistry globalComponentRegistry = ComponentRegistry.Instance;

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
        public void ShouldRegisterScriptingComponent()
        {
            Assert.IsNotNull(ComponentRegistry.Instance.FindComponentDefinition("scripting"));
        }
    }
}

