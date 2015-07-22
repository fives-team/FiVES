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
using FIVES;

namespace LightPlugin
{
    public class LightPluginInitializer : IPluginInitializer
    {
        #region Plugin Initializer Interface

        public string Name
        {
            get { return "Light"; }
        }

        public List<string> PluginDependencies
        {
            get { return new List<string>(); }
        }

        public List<string> ComponentDependencies
        {
            get { return new List<string>(); }
        }

        public void Initialize()
        {
            registerLightComponent();
        }

        public void Shutdown()
        {
        }

        #endregion

        private void registerLightComponent()
        {
            ComponentDefinition lightComponent = new ComponentDefinition("light");
            lightComponent.AddAttribute<LightType>("type");
            lightComponent.AddAttribute<Vector>("intensity");
            lightComponent.AddAttribute<Vector>("attenuation");
            ComponentRegistry.Instance.Register(lightComponent);
        }
    }
}
