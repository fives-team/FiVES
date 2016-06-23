// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using NLog;
using SINFONI;
using System.Configuration;
using System.Net;

namespace FIVES
{
    /// <summary>
    /// Plugin manager. Manages loading and initializing plugins.
    /// </summary>
    public class PluginManager
    {
        /// <summary>
        /// Default instance of the plugin manager. This should be used instead of creating a new instance.
        /// </summary>
        public readonly static PluginManager Instance = new PluginManager();

        /// <summary>
        /// Delegate to be used with <see cref="OnPluginInitialized"/>
        /// </summary>
        /// <param name="pluginName">Name of the initialized plugin</param>
        public delegate void PluginInitialized(Object sender, PluginInitializedEventArgs e);

        /// <summary>
        /// Occurs when a plugin is initialized.
        /// </summary>
        public event PluginInitialized OnAnyPluginInitialized;

        public PluginManager()
        {
            OnAnyPluginInitialized += HandleInitializedPlugin;

            ComponentRegistry.Instance.RegisteredComponent += HandleRegistredComponent;
        }

        public struct PluginInfo
        {
            public string Name;
            public string Path;
            public IPluginInitializer Initializer;
            public List<string> RemainingPluginDeps;
            public List<string> RemainingComponentDeps;
        }

        public ReadOnlyCollection<PluginInfo> LoadedPlugins
        {
            get
            {
                return new ReadOnlyCollection<PluginInfo>(loadedPlugins.Values);
            }
        }

        public ReadOnlyCollection<PluginInfo> DeferredPlugins
        {
            get
            {
                return new ReadOnlyCollection<PluginInfo>(deferredPlugins.Values);
            }
        }

        private List<string> attemptedFilenames = new List<string>();
        private Dictionary<string, PluginInfo> loadedPlugins = new Dictionary<string, PluginInfo>();
        private Dictionary<string, PluginInfo> deferredPlugins = new Dictionary<string, PluginInfo>();

        private static Logger Logger = LogManager.GetCurrentClassLogger();


        /// <summary>
        /// Canoninizes the filename (converts .. and . into actual path). This allows to identify plugin from the same
        /// file but different paths as the same. E.g. /foo/bar/baz/../plugin.dll is the same as /foo/bar/plugin.dll.
        /// </summary>
        /// <returns>The canonical path.</returns>
        /// <param name="path">The path to be canonized.</param>
        private string GetCanonicalPath(string path)
        {
            return Path.GetFullPath(path);
        }

        /// <summary>
        /// Attempts to load a plugin from the assembly located at <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path at which plugin assembly is to be found.</param>
        public void LoadPlugin(string path)
        {
            string canonicalPath = GetCanonicalPath(path);
            string name;
            if (!attemptedFilenames.Contains(canonicalPath))
            {
                try
                {
                    // Add this plugin to the list of loaded paths.
                    attemptedFilenames.Add(canonicalPath);

                    // Load an assembly.
                    Assembly assembly = Assembly.LoadFrom(canonicalPath);

                    // Find initializer class.
                    List<Type> types = new List<Type>(assembly.GetTypes());
                    Type interfaceType = typeof(IPluginInitializer);
                    Type initializerType = types.Find(t => interfaceType.IsAssignableFrom(t));
                    if (initializerType == null || initializerType.Equals(interfaceType))
                    {
                        Logger.Info("Assembly in file " + path +
                                    " doesn't contain any class implementing IPluginInitializer.");
                        return;
                    }

                    // Construct basic plugin info.
                    PluginInfo info = new PluginInfo();
                    info.Path = canonicalPath;
                    info.Initializer = (IPluginInitializer)Activator.CreateInstance(initializerType);

                    // Check if plugin with the same name was already loaded.
                    name = info.Initializer.Name;
                    info.Name = name;
                    if (loadedPlugins.ContainsKey(name))
                    {
                        Logger.Warn("Cannot load plugin from " + path + ". Plugin with the same name '" + name +
                                    "' was already loaded from " + loadedPlugins[name].Path + ".");
                        return;
                    }

                    // Check if plugin has all required dependencies.
                    info.RemainingPluginDeps =
                        info.Initializer.PluginDependencies.FindAll(plugin => !loadedPlugins.ContainsKey(plugin));
                    info.RemainingComponentDeps = info.Initializer.ComponentDependencies.FindAll(
                        component => ComponentRegistry.Instance.FindComponentDefinition(component) == null);

                    if (info.RemainingPluginDeps.Count > 0 || info.RemainingComponentDeps.Count > 0)
                    {
                        deferredPlugins.Add(name, info);
                        return;
                    }

                    try
                    {
                        // Initialize plugin.
                        info.Initializer.Initialize();
                    }
                    catch (HttpListenerException)
                    {
                        Logger.Error("Failed to initialize HttpListener in Plugin " + name + " from " + path + ". This problem may occur when the "
                            + " URL registered for the listener is not admitted to the invoking user. To solve this problem, FiVES can either "
                            + "be run with administrator privileges, or by configuring the respective URI to be allowed for any user using netsh. "
                            + "Please research for netsh urlacl configuration for exact details.");
                    }
                    catch (Exception e)
                    {
                        Logger.WarnException("Exception occured during initialization of " + name + " plugin.", e);
                        return;
                    }
                    loadedPlugins.Add(name, info);
                }
                catch (BadImageFormatException e)
                {
                    Logger.InfoException(path + " is not a valid assembly and thus cannot be loaded as a plugin.", e);
                    return;
                }
                catch (Exception e)
                {
                    Logger.WarnException("Failed to load file " + path + " as a plugin", e);
                    return;
                }

                if (OnAnyPluginInitialized != null)
                    OnAnyPluginInitialized(this, new PluginInitializedEventArgs(name));
            }
        }

        /// <summary>
        /// Updates deferred plugins by removing <paramref name="loadedPlugin"/> from the list of their remaining
        /// dependecies.
        /// </summary>
        /// <param name="loadedPlugin">Loaded plugin name.</param>
        private void HandleInitializedPlugin(Object sender, PluginInitializedEventArgs e)
        {
            // Iterate over deferred plugins and remove |loadedPlugin| from the list of dependencies.
            foreach (var info in deferredPlugins.Values)
                info.RemainingPluginDeps.Remove(e.pluginName);

            LoadDeferredPluginsWithNoDeps();
        }

        /// <summary>
        /// Updates deferred plugins by removing <paramref name="loadedPlugin"/> from the list of their remaining
        /// dependecies.
        /// </summary>
        /// <param name="loadedPlugin">Loaded plugin name.</param>
        private void HandleRegistredComponent(Object sender, RegisteredComponentEventArgs e)
        {
            // Iterate over deferred plugins and remove |loadedPlugin| from the list of dependencies.
            foreach (var info in deferredPlugins.Values)
                info.RemainingComponentDeps.Remove(e.ComponentDefinition.Name);

            LoadDeferredPluginsWithNoDeps();
        }

        /// <summary>
        /// Loads plugins from the deferred list that have no deps.
        /// </summary>
        private void LoadDeferredPluginsWithNoDeps()
        {
            // Find plugins that have no other dependencies.
            Dictionary<string, PluginInfo> pluginsWithNoDeps = new Dictionary<string, PluginInfo>();
            foreach (var plugin in deferredPlugins)
            {
                if (plugin.Value.RemainingPluginDeps.Count == 0 && plugin.Value.RemainingComponentDeps.Count == 0)
                    pluginsWithNoDeps.Add(plugin.Key, plugin.Value);
            }

            // Remove selected plugins from the deferred list.
            foreach (var entry in pluginsWithNoDeps)
                deferredPlugins.Remove(entry.Key);

            // Initialize these plugins and move them to loadedPlugins dictionary.
            foreach (var entry in pluginsWithNoDeps)
            {
                string name = entry.Key;
                PluginInfo pluginInfo = entry.Value;

                try
                {
                    pluginInfo.Initializer.Initialize();
                }
                catch (Exception ex)
                {
                    Logger.WarnException("Exception occured during initialization of " + name + " plugin.", ex);
                    deferredPlugins.Remove(name);
                    return;
                }

                loadedPlugins[name] = pluginInfo;
                if (OnAnyPluginInitialized != null)
                    OnAnyPluginInitialized(this, new PluginInitializedEventArgs(name));
            }
        }

        /// <summary>
        /// Attempts to load all valid plugins from the <paramref name="pluginDirectory"/>. By default all plugins in
        /// the directory are loaded. However, pluginWhiteList and pluginBlackList may be used to filter loaded plugins
        /// based on their filename. Both may be null and pluginWhiteList takes precendence when both are defined. In
        /// either case list is used as a set of substrings against which filename is checked.
        /// </summary>
        /// <param name="pluginDirectory">Directory in which plugins are too be looked for.</param>
        /// <param name="pluginWhiteList">List of plugins which can be loaded. Ignored if null.</param>
        /// <param name="pluginBlackList">List of plugins which must be ignored. Ignored if null.</param>
        public void LoadPluginsFrom(string pluginDirectory, string[] pluginWhiteList, string[] pluginBlackList)
        {
            string[] files = Directory.GetFiles(pluginDirectory, "*.dll");
            foreach (string filename in files)
            {
                string cleanFilename = Path.GetFileName(filename);
                if (pluginWhiteList != null)
                {
                    if (pluginWhiteList.Any(whiteListEntry => cleanFilename.Equals(whiteListEntry + ".dll")))
                        LoadPlugin(filename);
                }
                else if (pluginBlackList != null)
                {
                    if (!pluginBlackList.Any(blackListEntry => cleanFilename.Equals(blackListEntry + ".dll")))
                        LoadPlugin(filename);
                }
                else
                {
                    LoadPlugin(filename);
                }
            }
            ConfigurationManager.OpenExeConfiguration(this.GetType().Assembly.Location);
            string ServerIDLUri = ConfigurationManager.AppSettings["ServerIDL"];
            try
            {
                if (ServerIDLUri != null)
                {
                    World.Instance.SinTd = new IDLParser().ParseIDLFromUri(ServerIDLUri);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Failed to load IDL file from " + ServerIDLUri + ". Please make sure that "
                    + "SINFONI was initialized correctly, and the URI that is specified in the FiVES "
                    + "configuration points to the correct location of the IDL file. The exception thrown was: "
                    + e.Message);
            }
        }

        /// <summary>
        /// Returns whether plugin in assembly at <paramref name="path"/> was loaded and initialized.
        /// </summary>
        /// <returns><c>true</c>, if the plugin was initialized, <c>false</c> otherwise.</returns>
        /// <param name="path">The path to the assembly.</param>
        public bool IsPathLoaded(string path)
        {
            // Check if we've attempted loading this filename before.
            string canonicalPath = GetCanonicalPath(path);
            if (!attemptedFilenames.Contains(canonicalPath))
                return false;

            // Check if the plugin was loaded.
            foreach (var plugin in loadedPlugins)
            {
                if (plugin.Value.Path == canonicalPath)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether plugin with <paramref name="name"/> was loaded and initialized.
        /// </summary>
        /// <returns><c>true</c>, if the plugin was initialized, <c>false</c> otherwise.</returns>
        /// <param name="name">Plugin name.</param>
        public bool IsPluginLoaded(string name)
        {
            return loadedPlugins.ContainsKey(name);
        }

        /// <summary>
        /// Executes <paramref name="handler"/> when plugin with specified <paramref name="pluginName"/> is loaded. This
        /// can be used to add dynamic dependencies.
        /// </summary>
        /// <example>
        ///     PluginManager.Instance.AddPluginLoadedHandler("ClientSync", delegate() {
        ///         // do something that uses ClientSync plugin...
        ///     });
        /// </example>
        /// <param name="pluginName">Plugin to be loaded.</param>
        /// <param name="handler">Handler to be executed.</param>
        public void AddPluginLoadedHandler(string pluginName, Action handler)
        {
            if (IsPluginLoaded(pluginName))
            {
                handler();
            }
            else
            {
                PluginInitialized customPluginInitializedHandler = null;
                customPluginInitializedHandler = delegate(object sender, PluginInitializedEventArgs args)
                {
                    if (args.pluginName == pluginName)
                    {
                        OnAnyPluginInitialized -= customPluginInitializedHandler;
                        handler();
                    }
                };
                OnAnyPluginInitialized += customPluginInitializedHandler;
            }
        }

        /// <summary>
        /// Invokes the shutdown of all plug-ins in the order of their plugin dependencies, which means that plugins
        /// may  expect their dependencies to be still loaded when they are being shut down. This method should only be
        /// used upon server shutdown.
        /// </summary>
        public void ShutdownAllPlugins()
        {
            List<PluginInfo> pluginsToShutdown = new List<PluginInfo>(loadedPlugins.Values);
            while (pluginsToShutdown.Count > 0)
            {
                PluginInfo pluginOnWhichNothingDepends = pluginsToShutdown.Find(
                    p => pluginsToShutdown.All(p2 => !p2.Initializer.PluginDependencies.Contains(p.Name)));

                pluginOnWhichNothingDepends.Initializer.Shutdown();
                pluginsToShutdown.Remove(pluginOnWhichNothingDepends);
            }
        }
    }
}

