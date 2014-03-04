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
        private string pathToPlugins = "TestPlugins/";

        private IComponentRegistry globalComponentRegistry = ComponentRegistry.Instance;
        private World globalWorld = World.Instance;

        [SetUp()]
        public void Init()
        {
            ComponentRegistry.Instance = new ComponentRegistry();
            World.Instance = new World();
            pm = new PluginManager();
        }

        [TearDown()]
        public void ShutDown()
        {
            ComponentRegistry.Instance = globalComponentRegistry;
            World.Instance = globalWorld;
        }

        [Test()]
        public void ShouldLoadAllValidPluginsInDirectory()
        {
            pm.LoadPluginsFrom(pathToPlugins, null, null);
            Assert.IsTrue(pm.IsPathLoaded(pathToPlugins + "ValidPlugin1.dll"));
            Assert.IsTrue(pm.IsPathLoaded(pathToPlugins + "ValidPlugin2.dll"));
            Assert.IsTrue(pm.IsPluginLoaded("ValidPlugin1"));
            Assert.IsTrue(pm.IsPluginLoaded("ValidPlugin2"));
        }

        [Test()]
        public void ShouldOmitInvalidPluginsInDirectory()
        {
            pm.LoadPluginsFrom(pathToPlugins, null, null);
            Assert.IsFalse(pm.IsPathLoaded(pathToPlugins + "InValidPlugin1.dll"));
            Assert.IsFalse(pm.IsPathLoaded(pathToPlugins + "InvalidAssembly.dll"));
        }

        [Test()]
        public void ShouldUseWhiteList()
        {
            pm.LoadPluginsFrom(pathToPlugins, new string[] { "ValidPlugin1" }, null);
            Assert.IsFalse(pm.IsPathLoaded(pathToPlugins + "ValidPlugin2.dll"));
            Assert.IsTrue(pm.IsPathLoaded(pathToPlugins + "ValidPlugin1.dll"));
            Assert.IsTrue(pm.IsPluginLoaded("ValidPlugin1"));
        }

        [Test()]
        public void ShouldUseBlackList()
        {
            pm.LoadPluginsFrom(pathToPlugins, null, new string[] { "ValidPlugin2" });
            Assert.IsFalse(pm.IsPathLoaded(pathToPlugins + "ValidPlugin2.dll"));
            Assert.IsTrue(pm.IsPathLoaded(pathToPlugins + "ValidPlugin1.dll"));
            Assert.IsTrue(pm.IsPluginLoaded("ValidPlugin1"));
        }

        [Test()]
        public void ShouldRecognizeDifferentPathsToTheSamePlugin()
        {
            pm.LoadPlugin(pathToPlugins + "ValidPlugin1.dll");
            Assert.IsTrue(pm.IsPathLoaded(pathToPlugins + "../TestPlugins/ValidPlugin1.dll"));
            Assert.IsTrue(pm.IsPathLoaded(pathToPlugins + "./ValidPlugin1.dll"));
            Assert.IsTrue(pm.IsPluginLoaded("ValidPlugin1"));
        }

        [Test()]
        public void ShouldReturnNullOnMissingFile()
        {
            var testPlugin = pathToPlugins + "NonExistingFile.dll";
            pm.LoadPlugin(testPlugin);
            Assert.IsFalse(pm.IsPathLoaded(testPlugin));
        }

        [Test()]
        public void ShouldReturnNullOnInvalidPlugin()
        {
            var testPlugin = pathToPlugins + "InValidPlugin1.dll";
            pm.LoadPlugin(testPlugin);
            Assert.IsFalse(pm.IsPathLoaded(testPlugin));
        }

        [Test()]
        public void ShouldReturnNullOnInvalidAssembly()
        {
            var testPlugin = pathToPlugins + "InvalidAssembly.dll";
            pm.LoadPlugin(testPlugin);
            Assert.IsFalse(pm.IsPathLoaded(testPlugin));
        }

        [Test()]
        public void ShouldLoadPluginsInDependencyOrder()
        {
            pm.LoadPlugin(pathToPlugins + "ValidPlugin2.dll");
            Assert.IsFalse(pm.IsPathLoaded(pathToPlugins + "ValidPlugin2.dll"));
            Assert.IsFalse(pm.IsPluginLoaded("ValidPlugin2"));
            pm.LoadPlugin(pathToPlugins + "ValidPlugin1.dll");
            Assert.IsTrue(pm.IsPathLoaded(pathToPlugins + "ValidPlugin1.dll"));
            Assert.IsTrue(pm.IsPathLoaded(pathToPlugins + "ValidPlugin2.dll"));
            Assert.IsTrue(pm.IsPluginLoaded("ValidPlugin1"));
            Assert.IsTrue(pm.IsPluginLoaded("ValidPlugin2"));
        }

        [Test()]
        public void ShouldLoadPluginWhenComponentDependenciesAreSatisfied()
        {
            pm.LoadPlugin(pathToPlugins + "ValidPlugin3.dll");
            Assert.IsFalse(pm.IsPathLoaded(pathToPlugins + "ValidPlugin3.dll"));
            Assert.IsFalse(pm.IsPluginLoaded("ValidPlugin3"));
            ComponentRegistry.Instance.Register(new ComponentDefinition("testComponentForValidPlugin3"));
            Assert.IsTrue(pm.IsPathLoaded(pathToPlugins + "ValidPlugin3.dll"));
            Assert.IsTrue(pm.IsPluginLoaded("ValidPlugin3"));
        }
    }
}

