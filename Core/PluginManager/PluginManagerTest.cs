using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace FIVES
{
    [TestFixture()]
    public class PluginManagerTest
    {
        private PluginManager pm;
        private string pathToPlugins = "../../PluginManager/";

        [SetUp()]
        public void init()
        {
            pm = new PluginManager();
        }

        [Test()]
        public void shouldLoadAllValidPluginsInDirectory()
        {
            pm.loadPluginsFrom(pathToPlugins + "TestPlugins");
            Assert.IsTrue(pm.isPathLoaded(pathToPlugins + "TestPlugins/valid_plugin.dll"));
            Assert.IsTrue(pm.isPathLoaded(pathToPlugins + "TestPlugins/valid_plugin2.dll"));
            Assert.IsTrue(pm.isPluginLoaded("ValidPlugin1"));
            Assert.IsTrue(pm.isPluginLoaded("ValidPlugin2"));
        }

        [Test()]
        public void shouldOmitInvalidPluginsInDirectory()
        {
            pm.loadPluginsFrom(pathToPlugins + "TestPlugins");
            Assert.IsFalse(pm.isPathLoaded(pathToPlugins + "TestPlugins/invalid_plugin.dll"));
            Assert.IsFalse(pm.isPathLoaded(pathToPlugins + "TestPlugins/invalid_assembly.dll"));
        }

        [Test()]
        public void shouldRecognizeDifferentPathsToTheSamePlugin()
        {
            pm.loadPlugin(pathToPlugins + "TestPlugins/valid_plugin.dll");
            Assert.IsTrue(pm.isPathLoaded(pathToPlugins + "TestPlugins/../TestPlugins/valid_plugin.dll"));
            Assert.IsTrue(pm.isPathLoaded(pathToPlugins + "TestPlugins/./valid_plugin.dll"));
            Assert.IsTrue(pm.isPluginLoaded("ValidPlugin1"));
        }

        [Test()]
        public void shouldReturnNullOnMissingFile()
        {
            var testPlugin = pathToPlugins + "TestPlugins/non-existing-file.dll";
            pm.loadPlugin(testPlugin);
            Assert.IsFalse(pm.isPathLoaded(testPlugin));
        }

        [Test()]
        public void shouldReturnNullOnInvalidPlugin()
        {
            var testPlugin = pathToPlugins + "TestPlugins/invalid_plugin.dll";
            pm.loadPlugin(testPlugin);
            Assert.IsFalse(pm.isPathLoaded(testPlugin));
        }

        [Test()]
        public void shouldReturnNullOnInvalidAssembly()
        {
            var testPlugin = pathToPlugins + "TestPlugins/invalid_assembly.dll";
            pm.loadPlugin(testPlugin);
            Assert.IsFalse(pm.isPathLoaded(testPlugin));
        }

        [Test()]
        public void shouldLoadPluginsInDependencyOrder()
        {
            pm.loadPlugin(pathToPlugins + "TestPlugins/valid_plugin2.dll");
            Assert.IsFalse(pm.isPathLoaded(pathToPlugins + "TestPlugins/valid_plugin2.dll"));
            Assert.IsFalse(pm.isPluginLoaded("ValidPlugin2"));
            pm.loadPlugin(pathToPlugins + "TestPlugins/valid_plugin.dll");
            Assert.IsTrue(pm.isPathLoaded(pathToPlugins + "TestPlugins/valid_plugin.dll"));
            Assert.IsTrue(pm.isPathLoaded(pathToPlugins + "TestPlugins/valid_plugin2.dll"));
            Assert.IsTrue(pm.isPluginLoaded("ValidPlugin1"));
            Assert.IsTrue(pm.isPluginLoaded("ValidPlugin2"));
        }
    }
}

