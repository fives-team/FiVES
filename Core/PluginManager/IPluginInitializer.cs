using System;
using System.Collections.Generic;

namespace FIVES
{
    /// <summary>
    /// An interface that must be implemented by one class in a plugin. Plugin manager will find such class, create its 
    /// instance using <b>parameterless constructor</b> and use its methods to extra meta-info about and initialize 
    /// plugin. The plugin manager may construct the instance at any time, but will only call <see cref="initialize"/> 
    /// when all the dependency plugins identified by <see cref="getDependencies"/>  have been loaded.
    /// </summary>
    public interface IPluginInitializer
    {
        /// <summary>
        /// Rerturns the name of the plugin.
        /// </summary>
        /// <returns>The name of the plugin.</returns>
        string getName();

        /// <summary>
        /// Returns the list of names of the plugins that this plugin depends on.
        /// </summary>
        /// <returns>The list of names of the plugins that this plugin depends on.</returns>
        List<string> getDependencies();

        /// <summary>
        /// Initializes the plugin. This method will be called by the plugin manager when all dependency plugins have
        /// been loaded.
        /// </summary>
        void initialize();

    }
}

