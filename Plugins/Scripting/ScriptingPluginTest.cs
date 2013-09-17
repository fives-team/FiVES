using System;
using NUnit.Framework;

namespace Scripting
{
    [TestFixture()]
    public class ScriptingPluginTest
    {
        ScriptingPlugin plugin = new ScriptingPlugin();

        [Test()]
        public void ShouldReturnCorrectName()
        {
            Assert.AreEqual("Scripting", plugin.GetName());
        }

        [Test()]
        public void ShouldReturnCorrectDeps()
        {
            Assert.AreEqual(plugin.GetDependencies().Count, 1);
            Assert.Contains("DirectCall", plugin.GetDependencies());
        }
    }
}

