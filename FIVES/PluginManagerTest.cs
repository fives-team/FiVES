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
        private string pathToPlugins = "../../";

        [SetUp()]
        public void Init()
        {
            pm = new PluginManager();
        }

        [Test()]
        public void ShouldLoadAllValidPluginsInDirectory()
        {
            pm.LoadPluginsFrom(pathToPlugins + "TestPlugins");
            Assert.IsTrue(pm.IsPluginLoaded(pathToPlugins + "TestPlugins/valid_plugin.dll"));
            Assert.IsTrue(pm.IsPluginLoaded(pathToPlugins + "TestPlugins/valid_plugin2.dll"));
        }

        [Test()]
        public void ShouldOmitInvalidPluginsInDirectory()
        {
            pm.LoadPluginsFrom(pathToPlugins + "TestPlugins");
            Assert.IsFalse(pm.IsPluginLoaded(pathToPlugins + "TestPlugins/invalid_plugin.dll"));
            Assert.IsFalse(pm.IsPluginLoaded(pathToPlugins + "TestPlugins/invalid_assembly.dll"));
        }

        [Test()]
        public void ShouldRecognizeDifferentPathsToTheSamePlugin()
        {
            pm.LoadPlugin(pathToPlugins + "TestPlugins/valid_plugin.dll");
            Assert.IsTrue(pm.IsPluginLoaded(pathToPlugins + "TestPlugins/../TestPlugins/valid_plugin.dll"));
            Assert.IsTrue(pm.IsPluginLoaded(pathToPlugins + "TestPlugins/./valid_plugin.dll"));
        }

        [Test()]
        public void ShouldReturnNullOnMissingFile()
        {
            Assert.IsNull(pm.LoadPlugin(pathToPlugins + "TestPlugins/non-existing-file.dll"));
        }

        [Test()]
        public void ShouldReturnNullOnInvalidPlugin()
        {
            Assert.IsNull(pm.LoadPlugin(pathToPlugins + "TestPlugins/invalid_plugin.dll"));
        }

        [Test()]
        public void ShouldReturnNullOnInvalidAssembly()
        {
            Assert.IsNull(pm.LoadPlugin(pathToPlugins + "TestPlugins/invalid_assembly.dll"));
        }
    }
}

