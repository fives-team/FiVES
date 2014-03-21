using System;
using V8.Net;

namespace ScriptingPlugin
{
    /// <summary>
    /// Implementation of the <see cref="IJSContext"/> for <see cref="V8.Net.V8Engine"/>
    /// </summary>
    class V8NetContext : IJSContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptingPluginInitializer.V8NetContext"/> class.
        /// </summary>
        /// <param name="aEngine">Associated V8Engine.</param>
        public V8NetContext(V8Engine aEngine)
        {
            Engine = aEngine;
        }

        #region IJSContext implementation

        /// <summary>
        /// Executes the scripts.
        /// </summary>
        /// <param name="script">Script to be executed.</param>
        public object Execute(string script)
        {
            object result = null;
            Engine.WithContextScope = () => {
                result = Engine.Execute(script);
            };
            return result;
        }

        /// <summary>
        /// Registers a new global object in this context. The fields and methods of the new object correspond to the
        /// passed C# object.
        /// </summary>
        /// <param name="name">Name of the global object.</param>
        /// <param name="csObject">Corresponding C# object.</param>
        public void CreateGlobalObject(string name, object csObject)
        {
            Engine.WithContextScope = () => {
                Engine.GlobalObject.SetProperty(name, csObject, null, true, V8PropertyAttributes.Locked);
            };
        }

        #endregion

        internal V8Engine Engine;
    }
}
