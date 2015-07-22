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
using FIVES;
using System.Collections.Generic;

namespace ScriptingPlugin
{
    /// <summary>
    /// Scripting plugin.
    /// </summary>
    public class ScriptingPluginInitializer : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string Name
        {
            get
            {
                return "Scripting";
            }
        }

        public List<string> PluginDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        public List<string> ComponentDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Initializes the plugin. This method will be called by the plugin manager when all dependency plugins have
        /// been loaded.
        /// </summary>
        public void Initialize()
        {
            // Register 'scripting' component.
            ComponentDefinition scripting = new ComponentDefinition("scripting");
            scripting.AddAttribute<string>("ownerScript");
            scripting.AddAttribute<string>("serverScript");
            scripting.AddAttribute<string>("clientScript");
            ComponentRegistry.Instance.Register(scripting);

            Scripting.Instance = new Scripting();
        }

        public void Shutdown()
        {
        }

        #endregion
    }
}

