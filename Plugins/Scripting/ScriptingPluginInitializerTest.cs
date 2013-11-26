using System;
using NUnit.Framework;
using FIVES;

namespace ScriptingPlugin
{
    [TestFixture()]
    public class ScriptingPluginInitializerTest
    {
        ScriptingPluginInitializer plugin = new ScriptingPluginInitializer();

        public ScriptingPluginInitializerTest()
        {
            plugin.Initialize();
        }

        [Test()]
        public void ShouldRegisterScriptingComponent()
        {
            Assert.IsNotNull(ComponentRegistry.Instance.FindComponentDefinition("scripting"));
        }
    }
}

