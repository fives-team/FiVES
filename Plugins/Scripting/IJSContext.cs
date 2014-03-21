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

        /// <summary>
        /// Registers a new global object in this context. The fields and methods of the new object correspond to the
        /// passed C# object.
        /// </summary>
        /// <param name="name">Name of the global object.</param>
        /// <param name="csObject">Corresponding C# object.</param>
        void CreateGlobalObject(string name, object csObject);
    }
}

