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
            pm.LoadPluginsFrom(pathToPlugins + "TestPlugins");
            Assert.IsTrue(pm.IsPathLoaded(pathToPlugins + "TestPlugins/valid_plugin.dll"));
            Assert.IsTrue(pm.IsPathLoaded(pathToPlugins + "TestPlugins/valid_plugin2.dll"));
            Assert.IsTrue(pm.IsPluginLoaded("ValidPlugin1"));
            Assert.IsTrue(pm.IsPluginLoaded("ValidPlugin2"));
        }

        [Test()]
        public void shouldOmitInvalidPluginsInDirectory()
        {
            pm.LoadPluginsFrom(pathToPlugins + "TestPlugins");
            Assert.IsFalse(pm.IsPathLoaded(pathToPlugins + "TestPlugins/invalid_plugin.dll"));
            Assert.IsFalse(pm.IsPathLoaded(pathToPlugins + "TestPlugins/invalid_assembly.dll"));
        }

        [Test()]
        public void shouldRecognizeDifferentPathsToTheSamePlugin()
        {
            pm.LoadPlugin(pathToPlugins + "TestPlugins/valid_plugin.dll");
            Assert.IsTrue(pm.IsPathLoaded(pathToPlugins + "TestPlugins/../TestPlugins/valid_plugin.dll"));
            Assert.IsTrue(pm.IsPathLoaded(pathToPlugins + "TestPlugins/./valid_plugin.dll"));
            Assert.IsTrue(pm.IsPluginLoaded("ValidPlugin1"));
        }

        [Test()]
        public void shouldReturnNullOnMissingFile()
        {
            var testPlugin = pathToPlugins + "TestPlugins/non-existing-file.dll";
            pm.LoadPlugin(testPlugin);
            Assert.IsFalse(pm.IsPathLoaded(testPlugin));
        }

        [Test()]
        public void shouldReturnNullOnInvalidPlugin()
        {
            var testPlugin = pathToPlugins + "TestPlugins/invalid_plugin.dll";
            pm.LoadPlugin(testPlugin);
            Assert.IsFalse(pm.IsPathLoaded(testPlugin));
        }

        [Test()]
        public void shouldReturnNullOnInvalidAssembly()
        {
            var testPlugin = pathToPlugins + "TestPlugins/invalid_assembly.dll";
            pm.LoadPlugin(testPlugin);
            Assert.IsFalse(pm.IsPathLoaded(testPlugin));
        }

        [Test()]
        public void shouldLoadPluginsInDependencyOrder()
        {
            pm.LoadPlugin(pathToPlugins + "TestPlugins/valid_plugin2.dll");
            Assert.IsFalse(pm.IsPathLoaded(pathToPlugins + "TestPlugins/valid_plugin2.dll"));
            Assert.IsFalse(pm.IsPluginLoaded("ValidPlugin2"));
            pm.LoadPlugin(pathToPlugins + "TestPlugins/valid_plugin.dll");
            Assert.IsTrue(pm.IsPathLoaded(pathToPlugins + "TestPlugins/valid_plugin.dll"));
            Assert.IsTrue(pm.IsPathLoaded(pathToPlugins + "TestPlugins/valid_plugin2.dll"));
            Assert.IsTrue(pm.IsPluginLoaded("ValidPlugin1"));
            Assert.IsTrue(pm.IsPluginLoaded("ValidPlugin2"));
        }
    }
}

