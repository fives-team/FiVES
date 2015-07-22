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

namespace FIVES
{
    /// <summary>
    /// This interface is used internally to mock ComponentRegistry in unit tests. Please do not use it.
    /// </summary>
    public interface IComponentRegistry
    {
        ReadOnlyCollection<ReadOnlyComponentDefinition> RegisteredComponents { get; }
        void Register(ComponentDefinition definition);
        ReadOnlyComponentDefinition FindComponentDefinition(string componentName);
        event EventHandler<RegisteredComponentEventArgs> RegisteredComponent;
    }
}
