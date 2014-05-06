// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;

namespace KIARAPlugin
{
    abstract class Service : IService
    {
        /// <summary>
        /// Returns the associated context for this service.
        /// </summary>
        /// <value>The associated context.</value>
        public Context Context { get; private set; }

        public Delegate this[string name]
        {
            set
            {
                registeredMethods[name] = value;
            }
        }

        protected Service(Context aContext)
        {
            Context = aContext;
        }

        protected void RegisterMethods(Connection connection)
        {
            foreach (var entry in registeredMethods)
                connection.RegisterFuncImplementation(entry.Key, entry.Value);
        }

        Dictionary<string, Delegate> registeredMethods = new Dictionary<string, Delegate>();
    }
}

