using System;
using NUnit.Framework;
using FIVES;

namespace ScriptingPlugin
{
    [TestFixture()]
    public class ScriptingPluginInitializerTest
    {
        ScriptingPluginInitializer plugin = new ScriptingPluginInitializer();

        [Test()]
        public void ShouldReturnCorrectName()
        {
            Assert.AreEqual("Scripting", plugin.GetName());
        }

        [Test()]
        public void ShouldReturnCorrectDeps()
        {
            Assert.AreEqual(plugin.GetDependencies().Count, 0);
        }

        [Test()]
        public void ShouldRegisterScriptingComponent()
        {
            Assert.IsNotNull(ComponentRegistry.Instance.FindComponentDefinition("scripting"));
        }
    }
}

