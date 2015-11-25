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
using ClientManagerPlugin;
using FIVES;
using SINFONI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentInterfacePlugin
{
    public class ComponentInterfacePluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get { return "ComponentInterface"; }
        }

        public List<string> PluginDependencies
        {
            get { return new List<string>{"ClientManager"}; }
        }

        public List<string> ComponentDependencies
        {
            get { return new List<string>(); }
        }

        public void Initialize()
        {
            SINFONIPlugin.SINFONIServerManager.Instance.SinfoniServer.AmendIDL(File.ReadAllText("component.sinfoni"));
            RegisterSinfoniService();
        }

        public void Shutdown()
        {
        }

        private void RegisterSinfoniService()
        {
            ClientManager.Instance.RegisterClientService("component", true, new Dictionary<string, Delegate> {
                    { "registerComponent",
                        (Action<Connection, ExternalComponentDefinition>)HandleExternalRegisterRequest }
                });
        }

        private void HandleExternalRegisterRequest(Connection connection, ExternalComponentDefinition definition)
        {
            try
            {
                ComponentDefinition c = RegisterExternalComponent(connection, definition);
                registeredExternalComponents.Add(c);
            }
            catch(Exception e)
            {
                // SINFONI NEEDS A HANDLE TO PROCESS EXCEPTIONS TO THE CLIENT !!
                // https://github.com/tospie/SINFONI/issues/3
                // Until then, we log the error in the console
                Console.WriteLine("Cannot Register Component " + definition.Name + " : "
                    + e.Message + "\n" + e.InnerException);
            }
        }

        private ComponentDefinition RegisterExternalComponent(Connection conn, ExternalComponentDefinition definition)
        {
            ComponentDefinition c = new ComponentDefinition(definition.Name);
            foreach (ExternalAttributeDefinition attribute in definition.Attributes)
            {
                Type t = conn.SinTD.GetSinTDType(attribute.Type).InstanceType;
                c.AddAttribute(attribute.Name, t, attribute.DefaultValue);
            }
            ComponentRegistry.Instance.Register(c);
            return c;
        }

        HashSet<ComponentDefinition> registeredExternalComponents = new HashSet<ComponentDefinition>();
    }
}
