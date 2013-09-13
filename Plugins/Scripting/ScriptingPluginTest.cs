using System;
using NUnit.Framework;

namespace Scripting
{
    [TestFixture()]
    public class ScriptingPluginTest
    {
        ScriptingPlugin plugin = new ScriptingPlugin();

        [Test()]
        public void shouldReturnCorrectName()
        {
            Assert.AreEqual("Scripting", plugin.getName());
        }

        [Test()]
        public void shouldReturnCorrectDeps()
        {
            Assert.AreEqual(plugin.getDependencies().Count, 1);
            Assert.Contains("DirectCall", plugin.getDependencies());
        }
    }
}

