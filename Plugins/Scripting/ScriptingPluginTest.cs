using System;
using NUnit.Framework;

namespace ScriptingPlugin
{
    [TestFixture()]
    public class ScriptingPluginTest
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
    }
}

