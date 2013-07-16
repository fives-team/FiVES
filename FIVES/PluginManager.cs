using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NLog;

namespace FIVES
{
    class PluginManager
    {
        public static PluginManager Instance = new PluginManager();

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<string, object> loadedPluginInitializers = new Dictionary<string, object>();

        // Canoninizes the filename (converts .. and . into actual path). This allows to identify plugin from the same
        // file but different paths as the same. E.g. /foo/bar/baz/../plugin.dll is the same as /foo/bar/plugin.dll.
        private string GetCanonicalName(string filename)
        {
            return Path.GetFullPath(filename);
        }

        // Loads a plugin (unless loaded before) and returns plugin initializer. Returns null when loading fails.
        public object LoadPlugin(string filename)
        {
            string canonicalName = GetCanonicalName(filename);
            if (!loadedPluginInitializers.ContainsKey(canonicalName)) {
                try {
                    // Load an assembly.
                    Assembly assembly = Assembly.LoadFrom(canonicalName);
					
                    // Find initializer class.
                    Type initializer = assembly.GetType("PluginInitializer");
                    if (initializer == null) {
                        logger.Warn("Assembly in file " + filename + " doesn't contain PluginInitializer class.");
                        return null;
                    }
					
                    // Construct an instance of the initializer class.
                    loadedPluginInitializers[canonicalName] = Activator.CreateInstance(initializer);
                } catch (Exception e) {
                    logger.WarnException("Failed to open file " + filename + " as a plugin.", e);
                    return null;
                }
            }
            return loadedPluginInitializers[canonicalName];
        }

        public void LoadPluginsFrom(string pluginDirectory)
        {
            string[] files = Directory.GetFiles(pluginDirectory);
            foreach (string filename in files)
                LoadPlugin(filename);
        }

        public bool IsPluginLoaded(string filename)
        {
            string canonicalName = GetCanonicalName(filename);
            return loadedPluginInitializers.ContainsKey(canonicalName);
        }
    }
}

