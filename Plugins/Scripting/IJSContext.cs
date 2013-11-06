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

