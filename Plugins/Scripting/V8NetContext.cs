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
