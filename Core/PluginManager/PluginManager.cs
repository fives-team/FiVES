using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NLog;

namespace FIVES
{
    public class PluginManager
    {
        public static PluginManager Instance = new PluginManager();

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<string, object> loadedPluginInitializers = new Dictionary<string, object>();

        // Canoninizes the filename (converts .. and . into actual path). This allows to identify plugin from the same
        // file but different paths as the same. E.g. /foo/bar/baz/../plugin.dll is the same as /foo/bar/plugin.dll.
        private string getCanonicalName(string filename)
        {
            return Path.GetFullPath(filename);
        }

        // Loads a plugin (unless loaded before) and returns plugin initializer. Returns null when loading fails.
        public object loadPlugin(string filename)
        {
            string canonicalName = getCanonicalName(filename);
            if (!loadedPluginInitializers.ContainsKey(canonicalName)) {
                try {
                    // Load an assembly.
                    Assembly assembly = Assembly.LoadFrom(canonicalName);

                    // Find initializer class.
                    List<Type> types = new List<Type>(assembly.GetTypes());
                    Type pluginInitializerInterface = typeof(IPluginInitializer);
                    Type initializer = types.Find(t => pluginInitializerInterface.IsAssignableFrom(t));
                    if (initializer == null) {
                        logger.Warn("Assembly in file " + filename +
                                    " doesn't contain any class implementing IPluginInitializer.");
                        return null;
                    }

                    // Construct an instance of the initializer class.
                    loadedPluginInitializers[canonicalName] = Activator.CreateInstance(initializer);
                } catch (Exception e) {
                    logger.WarnException("Failed to load file " + filename + " as a plugin.", e);
                    return null;
                }
            }
            return loadedPluginInitializers[canonicalName];
        }

        public void loadPluginsFrom(string pluginDirectory)
        {
            string[] files = Directory.GetFiles(pluginDirectory);
            foreach (string filename in files)
                loadPlugin(filename);
        }

        public bool isPluginLoaded(string filename)
        {
            string canonicalName = getCanonicalName(filename);
            return loadedPluginInitializers.ContainsKey(canonicalName);
        }
    }
}

