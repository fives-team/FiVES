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
using System.Linq;
using System.Text;
using FIVES;

namespace FIVESServiceBus
{
    public class ServiceRegistry : IServiceRegistry
    {
        public void RegisterService(string serviceName, Func<AccumulatedAttributeTransform,
            AccumulatedAttributeTransform> transformFunction)
        {
            if(!registeredServiceFunctions.ContainsKey(serviceName))
                registeredServiceFunctions.Add(serviceName, transformFunction);
            else
                throw new Exception("Service with name '" + serviceName + "' is already registered");
        }

        public Func<AccumulatedAttributeTransform, AccumulatedAttributeTransform> Lookup(string serviceName)
        {
            if (registeredServiceFunctions.ContainsKey(serviceName))
                return registeredServiceFunctions[serviceName];
            else
                throw new Exception("Service with name '" + serviceName + "' is not registered");
        }

        IDictionary<string,
            Func<AccumulatedAttributeTransform, AccumulatedAttributeTransform>> registeredServiceFunctions
            = new Dictionary<string, Func<AccumulatedAttributeTransform, AccumulatedAttributeTransform>>();
    }
}
