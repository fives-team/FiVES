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

            deps = theDeps.Split(' ', ',').ToList();
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
            Engine.WithContextScope = () => {
                var res = Engine.Execute(script).AsString;
                //logger.Debug("Script: \n===\n" + script + "\n===\n" + res + "\n===\n");
            };

            CheckDeps();
        }

        /// <summary>
        /// Executes the script with given arguments, which are available as arguments[0], arguments[1] and so on.
        /// </summary>
        /// <param name="script">Script to be executed.</param>
        /// <param name="arguments">Arguments passed to the script.</param>
        public void Execute(string script, params object[] arguments)
        {
            Engine.WithContextScope = () => {
                InternalHandle argumentsArray = Engine.CreateArray();

                InternalHandle oldArguments = Engine.GlobalObject.GetProperty("arguments");
                Engine.GlobalObject.SetProperty("arguments", Engine.CreateValue(arguments));
                var res = Engine.Execute(script).AsString;
                //logger.Debug("Script: \n===\n" + script + "\n===\n" + res + "\n===\n");
                Engine.GlobalObject.SetProperty("arguments", oldArguments);
            };

            CheckDeps();
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

            CheckDeps();
        }

        #endregion

        private void CheckDeps()
        {
            if (!initialized)
            {
                Engine.WithContextScope = () =>
                {
                    deps = deps.FindAll(dep => Engine.GlobalObject.GetProperty(dep).IsUndefined);
                };

                if (deps.Count == 0)
                {
                    initialized = true; // this comes first to avoid infinite loop
                    Execute(initScript);
                }
            }
        }

        internal V8Engine Engine;
        List<string> deps;
        string initScript;
        bool initialized;

        static Logger logger = LogManager.GetCurrentClassLogger();
    }
}
