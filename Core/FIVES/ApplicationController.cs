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
using System.Linq;
using System.Text;
using System.Threading;

namespace FIVES
{
    /// <summary>
    /// Allows to control the application in a plug-in.
    /// </summary>
    public class ApplicationController
    {
        /// <summary>
        /// Initiates application shutdown procedure, which unloads all plugins.
        /// </summary>
        public void Terminate()
        {
            applicationTerminated.Set();
        }

        /// <summary>
        /// Should be set to true, when some plugin controls the application. Otherwise, the application will use
        /// default controlling mechanism.
        /// </summary>
        public bool ControlTaken { get; set; }

        /// <summary>
        /// An event which is raised after all plugins in the plugin directory have been loaded. This does not mean,
        /// however, that all of these plugins were initialized as they may have remaining dependencies, which may be
        /// satisfied later. Also additional plugins may still be loaded via the PluginManager class.
        /// </summary>
        public event EventHandler PluginsLoaded;

        internal void WaitForTerminate()
        {
            applicationTerminated.WaitOne();
        }

        internal void NotifyPluginsLoaded()
        {
            if (PluginsLoaded != null)
                PluginsLoaded(this, new EventArgs());
        }

        private AutoResetEvent applicationTerminated = new AutoResetEvent(false);
    }
}
