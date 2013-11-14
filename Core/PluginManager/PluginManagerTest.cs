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
            Assert.IsTrue(pm.IsPathLoaded(pathToPlugins + "TestPlugins/ValidPlugin1.dll"));
            Assert.IsTrue(pm.IsPathLoaded(pathToPlugins + "TestPlugins/ValidPlugin2.dll"));
            Assert.IsTrue(pm.IsPluginLoaded("ValidPlugin1"));
            Assert.IsTrue(pm.IsPluginLoaded("ValidPlugin2"));
        }

        [Test()]
        public void ShouldOmitInvalidPluginsInDirectory()
        {
            pm.LoadPluginsFrom(pathToPlugins + "TestPlugins");
            Assert.IsFalse(pm.IsPathLoaded(pathToPlugins + "TestPlugins/InValidPlugin1.dll"));
            Assert.IsFalse(pm.IsPathLoaded(pathToPlugins + "TestPlugins/InvalidAssembly.dll"));
        }

        [Test()]
        public void ShouldRecognizeDifferentPathsToTheSamePlugin()
        {
            pm.LoadPlugin(pathToPlugins + "TestPlugins/ValidPlugin1.dll");
            Assert.IsTrue(pm.IsPathLoaded(pathToPlugins + "TestPlugins/../TestPlugins/ValidPlugin1.dll"));
            Assert.IsTrue(pm.IsPathLoaded(pathToPlugins + "TestPlugins/./ValidPlugin1.dll"));
            Assert.IsTrue(pm.IsPluginLoaded("ValidPlugin1"));
        }

        [Test()]
        public void ShouldReturnNullOnMissingFile()
        {
            var testPlugin = pathToPlugins + "TestPlugins/NonExistingFile.dll";
            pm.LoadPlugin(testPlugin);
            Assert.IsFalse(pm.IsPathLoaded(testPlugin));
        }

        [Test()]
        public void ShouldReturnNullOnInvalidPlugin()
        {
            var testPlugin = pathToPlugins + "TestPlugins/InValidPlugin1.dll";
            pm.LoadPlugin(testPlugin);
            Assert.IsFalse(pm.IsPathLoaded(testPlugin));
        }

        [Test()]
        public void ShouldReturnNullOnInvalidAssembly()
        {
            var testPlugin = pathToPlugins + "TestPlugins/InvalidAssembly.dll";
            pm.LoadPlugin(testPlugin);
            Assert.IsFalse(pm.IsPathLoaded(testPlugin));
        }

        [Test()]
        public void ShouldLoadPluginsInDependencyOrder()
        {
            pm.LoadPlugin(pathToPlugins + "TestPlugins/ValidPlugin2.dll");
            Assert.IsFalse(pm.IsPathLoaded(pathToPlugins + "TestPlugins/ValidPlugin2.dll"));
            Assert.IsFalse(pm.IsPluginLoaded("ValidPlugin2"));
            pm.LoadPlugin(pathToPlugins + "TestPlugins/ValidPlugin1.dll");
            Assert.IsTrue(pm.IsPathLoaded(pathToPlugins + "TestPlugins/ValidPlugin1.dll"));
            Assert.IsTrue(pm.IsPathLoaded(pathToPlugins + "TestPlugins/ValidPlugin2.dll"));
            Assert.IsTrue(pm.IsPluginLoaded("ValidPlugin1"));
            Assert.IsTrue(pm.IsPluginLoaded("ValidPlugin2"));
        }

        [Test()]
        public void ShouldCorrectlyLoadTwoPluginsWhoseDepsWereResolvedAtTheSameTime()
        {
            // TODO
        }
    }
}

