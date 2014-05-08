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

namespace KIARAPlugin
{
    public static class ServiceFactory
    {
        /// <summary>
        /// Creates a new service with description at <paramref name="configURI"/> using the default context.
        /// </summary>
        /// <returns>Created service.</returns>
        /// <param name="configURI">Configuration URI.</param>
        public static IServiceImpl Create(string configURI)
        {
            ServiceImpl service = new ServiceImpl(Context.DefaultContext);
            Context.DefaultContext.StartServer(configURI, service.HandleNewClient);
            return service;
        }

        public static IServiceWrapper Discover(string configURI)
        {
            ServiceWrapper service = new ServiceWrapper(Context.DefaultContext);
            Context.DefaultContext.OpenConnection(configURI, service.HandleConnected);
            return service;
        }
    }
}

