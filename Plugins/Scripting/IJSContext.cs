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

namespace ScriptingPlugin
{
    /// <summary>
    /// Interface to be implemented by JavaScript context when passed to other plugins.
    /// </summary>
    public interface IJSContext
    {
        /// <summary>
        /// Executes the scripts.
        /// </summary>
        /// <param name="script">Script to be executed.</param>
        object Execute(string script);
    }
}

