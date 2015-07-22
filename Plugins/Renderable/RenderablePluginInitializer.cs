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
using ClientManagerPlugin;

namespace RenderablePlugin
{
    public class RenderablePluginInitializer : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string Name
        {
            get
            {
                return "Renderable";
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

        public void Initialize()
        {
            DefineComponents();
        }

        public void Shutdown()
        {
        }

        private void DefineComponents()
        {
            ComponentDefinition mesh = new ComponentDefinition("mesh");
            mesh.AddAttribute<string>("uri");
            mesh.AddAttribute<bool>("visible", true);
            mesh.AddAttribute<Vector>("scale", new Vector(1, 1, 1));
            ComponentRegistry.Instance.Register(mesh);
        }

        #endregion
    }
}

