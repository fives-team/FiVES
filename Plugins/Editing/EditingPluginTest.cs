
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
            Assert.AreEqual("Editing", plugin.getName());
        }

        [Test()]
        public void shouldReturnCorrectDeps()
        {
            Assert.AreEqual(plugin.getDependencies().Count, 1);
            Assert.Contains("Location", plugin.getDependencies());
        }
    }
}

