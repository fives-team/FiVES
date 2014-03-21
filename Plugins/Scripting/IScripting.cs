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
        /// Registers a new global object, which is created in all contexts. The fields and methods of the new object
        /// correspond to the passed C# object.
        /// </summary>
        /// <param name="name">Name of the global object.</param>
        /// <param name="csObject">Corresponding C# object.</param>
        void RegisterGlobalObject(string name, object csObject);

        /// <summary>
        /// Triggered when a new scripting context has been created, which is done for each entity which has a server
        /// script. This can be used to perform non-trivial initialization of the scripting API.
        /// </summary>
        event EventHandler<NewContextCreatedArgs> NewContextCreated;
    }
}
