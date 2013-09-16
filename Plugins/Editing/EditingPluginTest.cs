
using NUnit.Framework;
using System;

namespace Editing
{
    [TestFixture()]
    public class EditingPluginTest
    {
        EditingPlugin plugin = new EditingPlugin();

        [Test()]
        public void shouldReturnCorrectName()
        {
            Assert.AreEqual("Editing", plugin.GetName());
        }

        [Test()]
        public void shouldReturnCorrectDeps()
        {
            Assert.AreEqual(plugin.GetDependencies().Count, 1);
            Assert.Contains("Location", plugin.GetDependencies());
        }
    }
}

