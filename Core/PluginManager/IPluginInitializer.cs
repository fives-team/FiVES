using System;
using System.Collections.Generic;

namespace FIVES
{
    /// <summary>
    /// An interface that must be implemented by one class in a plugin.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Plugin manager will find such class, create its instance using <b>parameterless constructor</b> and use its
    /// methods to extract meta-info about and initialize the plugin. The plugin manager may construct the instance at
    /// any time, but will only call <see cref="initialize"/> when all the plugins with names returned by
    /// <see cref="getDependencies"/> have been loaded and initialized.
    /// </para>
    /// <para>
    /// Note that there is no protection or detection of circular dependencies of the plugins. It is up to the
    /// developers to ensure that there no such dependencies. If such a dependecy loop is present, none of these plugins
    /// will be initialized. Similarly, if a plugin depends on another plugin that is never initialized or loaded, the
    /// first plugin will never be initialized either.
    /// </para>
    /// </remarks>
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

