using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptingPlugin
{
    /// <summary>
    /// The scripting plugin interface.
    /// </summary>
    public interface IScripting
    {
        /// <summary>
        /// Registers a new global object in the script context. The fields and methods of the new object correspond to the
        /// passed C# object.
        /// </summary>
        /// <param name="name">Name of the global object.</param>
        /// <param name="csObject">Corresponding C# object.</param>
        void RegisterGlobalObject(string name, object csObject);

        /// <summary>
        /// Adds a new handler which is invoked each time a new context is create, which happens every time a new scripted
        /// object is discovered.
        /// </summary>
        /// <param name="handler">Handler to be invoked.</param>
        void AddNewContextHandler(Action<IJSContext> handler);
    }
}
