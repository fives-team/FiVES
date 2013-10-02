using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NLog;
using Events;

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
            OnAnyPluginInitialized += UpdateDeferredPlugins;
            OnAnyPluginInitialized += (sender, e) => Logger.Debug("Loaded plugin {0}", e.pluginName);
        }

        private struct LoadedPluginInfo {
            public string path;
            public IPluginInitializer initializer;
            public List<string> remainingDeps;
        }

        private List<string> AttemptedFilenames = new List<string>();
        private Dictionary<string, LoadedPluginInfo> LoadedPlugins = new Dictionary<string, LoadedPluginInfo>();
        private Dictionary<string, LoadedPluginInfo> DeferredPlugins = new Dictionary<string, LoadedPluginInfo>();

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
            if (!AttemptedFilenames.Contains(canonicalPath)) {
                try {
                    // Add this plugin to the list of loaded paths.
                    AttemptedFilenames.Add(canonicalPath);

                    // Load an assembly.
                    Assembly assembly = Assembly.LoadFrom(canonicalPath);

                    // Find initializer class.
                    List<Type> types = new List<Type>(assembly.GetTypes());
                    Type initializerType = types.Find(t => typeof(IPluginInitializer).IsAssignableFrom(t));
                    if (initializerType == null) {
                        Logger.Warn("Assembly in file " + path +
                                    " doesn't contain any class implementing IPluginInitializer.");
                        return;
                    }

                    // Construct basic plugin info.
                    LoadedPluginInfo info = new LoadedPluginInfo();
                    info.path = canonicalPath;
                    info.initializer = (IPluginInitializer)Activator.CreateInstance(initializerType);

                    // Check if plugin with the same name was already loaded.
                    string name = info.initializer.GetName();
                    if (LoadedPlugins.ContainsKey(name)) {
                        Logger.Warn("Cannot load plugin from " + path + ". Plugin with the same name '" + name +
                                    "' was already loaded from " + LoadedPlugins[name].path + ".");
                        return;
                    }

                    // Check if plugin has all required dependencies.
                    var dependencies = info.initializer.GetDependencies();
                    info.remainingDeps = dependencies.FindAll(depencency => !LoadedPlugins.ContainsKey(depencency));
                    if (info.remainingDeps.Count > 0) {
                        DeferredPlugins.Add(name, info);
                        return;
                    }

                    try {
                        // Initialize plugin.
                        info.initializer.Initialize();
                    } catch (Exception e) {
                        Logger.ErrorException("Exception occured during initialization of " + name + " plugin.", e);
                        return;
                    }
                    LoadedPlugins.Add(name, info);
                    if (OnAnyPluginInitialized != null)
                        OnAnyPluginInitialized(this, new PluginInitializedEventArgs(name));
                } catch (Exception e) {
                    Logger.WarnException("Failed to load file " + path + " as a plugin", e);
                    return;
                }
            }
        }

        /// <summary>
        /// Updates deferred plugins by removing <paramref name="loadedPlugin"/> from the list of their remaining
        /// dependecies. Plugins that have no other remaining dependencies are initialized.
        /// </summary>
        /// <param name="loadedPlugin">Loaded plugin name.</param>
        private void UpdateDeferredPlugins(Object sender, PluginInitializedEventArgs e)
        {
            // Iterate over deferred plugins and remove |loadedPlugin| from the list of dependencies.
            foreach (var info in DeferredPlugins.Values)
                info.remainingDeps.Remove(e.pluginName);

            // Find plugins that have no other dependencies.
            List<string> pluginsWithNoDeps = new List<string>();
            foreach (var plugin in DeferredPlugins) {
                if (plugin.Value.remainingDeps.Count == 0)
                    pluginsWithNoDeps.Add(plugin.Key);
            }

            // Initialize these plugins and move them to loadedPlugins dictionary.
            foreach (var name in pluginsWithNoDeps) {
                DeferredPlugins[name].initializer.Initialize();
                LoadedPlugins[name] = DeferredPlugins[name];
                DeferredPlugins.Remove(name);
                if (OnAnyPluginInitialized != null)
                    OnAnyPluginInitialized(this, new PluginInitializedEventArgs(name));
            }
        }

        /// <summary>
        /// Attempts to load all valid plugins from the <paramref name="pluginDirectory"/>.
        /// </summary>
        /// <param name="pluginDirectory">Directory in which plugins are too be looked for.</param>
        public void LoadPluginsFrom(string pluginDirectory)
        {
            string[] files = Directory.GetFiles(pluginDirectory, "*.dll");
            foreach (string filename in files)
                LoadPlugin(filename);
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
            if (!AttemptedFilenames.Contains(canonicalPath))
                return false;

            // Check if the plugin was loaded.
            foreach (var plugin in LoadedPlugins) {
                if (plugin.Value.path == canonicalPath)
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
            return LoadedPlugins.ContainsKey(name);
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
            if (IsPluginLoaded(pluginName)) {
                handler();
            } else {
                PluginInitialized customPluginInitializedHandler = null;
                customPluginInitializedHandler = delegate(object sender, PluginInitializedEventArgs args) {
                    if (args.pluginName == pluginName) {
                        OnAnyPluginInitialized -= customPluginInitializedHandler;
                        handler();
                    }
                };
                OnAnyPluginInitialized += customPluginInitializedHandler;
            }
        }

    }
}

