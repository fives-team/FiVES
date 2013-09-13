
using NUnit.Framework;
using System;

namespace Editing
{
    [TestFixture()]
    public class EditingPluginTest
    {
        EditingPlugin plugin = new EditingPlugin();

        [Test()]
        public void ShouldReturnCorrectName()
        {
            Assert.AreEqual("Editing", plugin.GetName());
        }

        [Test()]
        public void ShouldReturnCorrectDeps()
        {
            Assert.AreEqual(plugin.GetDependencies().Count, 2);
            Assert.Contains("Location", plugin.GetDependencies());
            Assert.Contains("DirectCall", plugin.GetDependencies());
        }
    }
}

