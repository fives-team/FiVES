using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public V8NetContext(V8Engine aEngine, string anInitScript, string theDeps)
        {
            Engine = aEngine;

            deps = theDeps;
            initScript = anInitScript;
            initialized = false;
            CheckDeps();
        }

        #region IJSContext implementation

        /// <summary>
        /// Executes the scripts.
        /// </summary>
        /// <param name="script">Script to be executed.</param>
        public void Execute(string script)
        {
            Engine.Execute(script);
            CheckDeps();
        }

        /// <summary>
        /// Executes the script with given arguments, which are available as arguments[0], arguments[1] and so on.
        /// </summary>
        /// <param name="script">Script to be executed.</param>
        /// <param name="arguments">Arguments passed to the script.</param>
        public void Execute(string script, params object[] arguments)
        {
            InternalHandle argumentsArray = Engine.CreateArray();

            InternalHandle oldArguments = Engine.GlobalObject.GetProperty("arguments");
            Engine.GlobalObject.SetProperty("arguments", Engine.CreateValue(arguments));
            Engine.Execute(script);
            Engine.GlobalObject.SetProperty("arguments", oldArguments);

            CheckDeps();
        }

        /// <summary>
        /// Registers a new global object in this context. The fields and methods of the new object correspond to the
        /// passed C# object.
        /// </summary>
        /// <param name="name">Name of the global object.</param>
        /// <param name="obj">Corresponding C# object.</param>
        public void CreateGlobalObject(string name, object obj)
        {
            if (obj != null)
                obj = V8NetObjectWrapper.Wrap(Engine, obj);
            Engine.GlobalObject.SetProperty(name, obj, null, true, ScriptMemberSecurity.Locked);

            CheckDeps();
        }

        /// <summary>
        /// Defines a function at global scope which can be used to construct an object of given type.
        /// </summary>
        /// <param name="type">Type to be constructed.</param>
        /// <param name="scriptTypeName">Name of the type in the script. When null, original type name is used.</param>
        public void RegisterTypeConstructors(Type type, string scriptTypeName = null)
        {
            Engine.GlobalObject.SetProperty(type, V8PropertyAttributes.Locked, scriptTypeName, true,
                ScriptMemberSecurity.Locked);
        }

        #endregion

        private void CheckDeps()
        {
            if (!initialized)
            {
                InternalHandle handle = Engine.Execute(deps);
                if (!handle.IsError && handle.IsBoolean && handle.AsBoolean)
                {
                    initialized = true; // this comes first to avoid infinite loop
                    Execute(initScript);
                }
            }
        }

        internal V8Engine Engine;
        string deps;
        string initScript;
        bool initialized;

        static Logger logger = LogManager.GetCurrentClassLogger();
    }
}
