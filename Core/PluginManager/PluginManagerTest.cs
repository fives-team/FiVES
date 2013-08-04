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
            Assert.IsTrue(pm.isPluginLoaded(pathToPlugins + "TestPlugins/valid_plugin.dll"));
            Assert.IsTrue(pm.isPluginLoaded(pathToPlugins + "TestPlugins/valid_plugin2.dll"));
        }

        [Test()]
        public void shouldOmitInvalidPluginsInDirectory()
        {
            pm.loadPluginsFrom(pathToPlugins + "TestPlugins");
            Assert.IsFalse(pm.isPluginLoaded(pathToPlugins + "TestPlugins/invalid_plugin.dll"));
            Assert.IsFalse(pm.isPluginLoaded(pathToPlugins + "TestPlugins/invalid_assembly.dll"));
        }

        [Test()]
        public void shouldRecognizeDifferentPathsToTheSamePlugin()
        {
            pm.loadPlugin(pathToPlugins + "TestPlugins/valid_plugin.dll");
            Assert.IsTrue(pm.isPluginLoaded(pathToPlugins + "TestPlugins/../TestPlugins/valid_plugin.dll"));
            Assert.IsTrue(pm.isPluginLoaded(pathToPlugins + "TestPlugins/./valid_plugin.dll"));
        }

        [Test()]
        public void shouldReturnNullOnMissingFile()
        {
            var testPlugin = pathToPlugins + "TestPlugins/non-existing-file.dll";
            pm.loadPlugin(testPlugin);
            Assert.IsFalse(pm.isPluginLoaded(testPlugin));
        }

        [Test()]
        public void shouldReturnNullOnInvalidPlugin()
        {
            var testPlugin = pathToPlugins + "TestPlugins/invalid_plugin.dll";
            pm.loadPlugin(testPlugin);
            Assert.IsFalse(pm.isPluginLoaded(testPlugin));
        }

        [Test()]
        public void shouldReturnNullOnInvalidAssembly()
        {
            var testPlugin = pathToPlugins + "TestPlugins/invalid_assembly.dll";
            pm.loadPlugin(testPlugin);
            Assert.IsFalse(pm.isPluginLoaded(testPlugin));
        }

        [Test()]
        public void shouldLoadPluginsInDependencyOrder()
        {
            pm.loadPlugin(pathToPlugins + "TestPlugins/valid_plugin2.dll");
            Assert.False(pm.isPluginLoaded(pathToPlugins + "TestPlugins/valid_plugin2.dll"));
            pm.loadPlugin(pathToPlugins + "TestPlugins/valid_plugin.dll");
            Assert.True(pm.isPluginLoaded(pathToPlugins + "TestPlugins/valid_plugin.dll"));
            Assert.True(pm.isPluginLoaded(pathToPlugins + "TestPlugins/valid_plugin2.dll"));
        }
    }
}

