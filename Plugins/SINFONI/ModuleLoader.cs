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
using SINFONI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SINFONIPlugin
{
    public class ModuleLoader
    {
        public void LoadModulesFrom<T>(string directoryName)
        {
            string[] files = Directory.GetFiles(directoryName, "*.dll");
            foreach (string filename in files)
            {
                string cleanFilename = Path.GetFileName(filename);
                LoadModule<T>(filename);
            }
        }

        public void LoadModule<T>(string fileName)
        {
            try
            {
                Type moduleType = findModuleTypeInFile<T>(fileName);
                var module = (T)Activator.CreateInstance(moduleType);

                if (typeof(T).Equals(typeof(IProtocol)))
                    ProtocolRegistry.Instance.RegisterProtocol((IProtocol)module);

                else if(typeof(T).Equals(typeof(ITransport)))
                    TransportRegistry.Instance.RegisterTransport((ITransport)module);
            }
            catch (InvalidCastException)
            {
                // Failed loading of .dll as modules should not break application.
                // Especially when putting transports and protocols in same location as plugins, this may happen
                // frequently
            }
            catch (Exception e)
            {
                // Other exceptions should be handled, though
                Console.WriteLine("[SINFONI] An exception occured when trying to load module "
                    + fileName + " into SINFONI: " + e.Message);
            }
        }

        private Type findModuleTypeInFile<T>(string fileName)
        {
                Assembly moduleAssembly = Assembly.LoadFrom(fileName);
                Type interfaceType = typeof(T);
                List<Type> typesInAssembly = new List<Type>(moduleAssembly.GetTypes());
                Type moduleType = typesInAssembly.Find(t => interfaceType.IsAssignableFrom(t));
                if (moduleType == null || moduleType.Equals(typeof(T)))
                    throw new InvalidCastException("File " + fileName +
                        " contains no valid implementation of " + typeof(T).ToString());
                return moduleType;
        }
    }
}
