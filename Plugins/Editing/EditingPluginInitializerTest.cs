
using NUnit.Framework;
using System;

namespace EditingNamespace
{
    [TestFixture()]
    public class EditingPluginInitializerTest
    {
        EditingPluginInitializer plugin = new EditingPluginInitializer();

        [Test()]
        public void ShouldReturnCorrectName()
        {
            Assert.AreEqual("Editing", plugin.GetName());
        }

        [Test()]
        public void ShouldReturnCorrectDeps()
        {
            Assert.AreEqual(plugin.GetDependencies().Count, 1);
            Assert.Contains("Renderable", plugin.GetDependencies());
        }
    }
}

