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
        /// The name of the plugin.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// List of names of the plugins on whose functionality (classes) this plugin depends on.
        /// </summary>
        List<string> PluginDependencies { get; }

        /// <summary>
        /// List of names of the plugins on whose functionality (classes) this plugin depends on.
        /// </summary>
        List<string> ComponentDependencies { get; }

        /// <summary>
        /// Initializes the plugin. This method will be called by the plugin manager when all dependency plugins have
        /// been satisfied.
        /// </summary>
        void Initialize();

        /// <summary>
        /// This method will be executed when the server is shutting down. Plug-ins may expect to have all plug-ins
        /// which they depend upon to be still loaded while this method is invoked, but such plug-ins may be unloaded
        /// immediately after this function returns. As we can not completely eliminate the possibility of the server
        /// crashes, it is also recommended for the developers to introduce other mechanisms that continuously persist
        /// critical data to reduce the data lost in such an event.
        /// </summary>
        void Shutdown();
    }
}

