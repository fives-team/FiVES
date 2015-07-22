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
using V8.Net;

namespace ScriptingPlugin
{
    /// <summary>
    /// Implementation of the <see cref="IJSContext"/> for <see cref="V8.Net.V8Engine"/>
    /// </summary>
    public class V8NetContext : IJSContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptingPluginInitializer.V8NetContext"/> class.
        /// </summary>
        /// <param name="aEngine">Associated V8Engine.</param>
        public V8NetContext(V8Engine aEngine)
        {
            engine = aEngine;
        }

        #region IJSContext implementation

        /// <summary>
        /// Executes the scripts.
        /// </summary>
        /// <param name="script">Script to be executed.</param>
        public object Execute(string script)
        {
            object result = null;
            engine.WithContextScope = () => {
                result = engine.Execute(script);
            };
            return result;
        }

        #endregion

        private V8Engine engine;
    }
}
