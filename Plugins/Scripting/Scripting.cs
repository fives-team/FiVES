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
using FIVES;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8.Net;

namespace ScriptingPlugin
{
    public class Scripting
    {
        public static Scripting Instance;

        public Scripting()
        {
            World.Instance.AddedEntity += HandleOnEntityAdded;

            // Register event handlers for all entities that were created before this plugin was loaded.
            foreach (var entity in World.Instance)
                HandleOnEntityAdded(World.Instance, new EntityEventArgs(entity));

            // FIXME: This is for debugging purposes only. Remove in release.
            registeredGlobalObjects["console"] = new CLRConsole();
        }

        /// <summary>
        /// Registers a new global object in the script context. The fields and methods of the new object correspond to the
        /// passed C# object.
        /// </summary>
        /// <param name="name">Name of the global object.</param>
        /// <param name="csObject">Corresponding C# object.</param>
        public void RegisterGlobalObject(string name, object csObject)
        {
            if (registeredGlobalObjects.ContainsKey(name))
                throw new Exception("The object with " + name + " is already registered");
            registeredGlobalObjects[name] = csObject;
        }

        /// <summary>
        /// Adds a new handler which is invoked each time a new context is create, which happens every time a new scripted
        /// object is discovered.
        /// </summary>
        /// <param name="handler">Handler to be invoked.</param>
        public void AddNewContextHandler(Action<IJSContext> handler)
        {
            newContextHandlers.Add(handler);
        }

        private void HandleOnEntityAdded(object sender, EntityEventArgs e)
        {
            // FIXME: This is not very efficient. Would be nice to be triggered only when anything in "scripting" 
            // component is changed, but without the need to create one. Essentially we need an event
            // OnComponentCreated.
            e.Entity.ChangedAttribute += HandleOnAttributeInComponentChanged;
            InitEntityContext(e.Entity);
        }

        private void HandleOnAttributeInComponentChanged(object sender, ChangedAttributeEventArgs e)
        {
            if (e.Component.Name == "scripting")
                InitEntityContext((Entity)sender);
        }

        private void InitEntityContext(Entity entity)
        {
            // Remove previous context if any.
            entityContexts.Remove(entity.Guid);

            logger.Debug("Entered initEntityContext with entity [{0}]", entity.Guid);

            string serverScript = (string)entity["scripting"]["serverScript"].Value;
            if (serverScript == null)
                return;

            logger.Debug("Creating the context. Server script is {0}", serverScript);

            V8Engine engine;
            try
            {
                engine = new V8Engine();
            }
            catch (Exception e)
            {
                logger.ErrorException("Exception during context creation", e);
                return;
            }

            logger.Debug("Adding context to the list");

            entityContexts.Add(entity.Guid, engine);

            logger.Debug("Creating context wrapper");

            // This object should be used to assign event handlers, e.g. script.onNewObject = function (newObject) {...}
            var context = new V8NetContext(engine);

            logger.Debug("Creating script object");

            context.Execute("script = {}");

            logger.Debug("About to enter context scope");

            engine.WithContextScope = () =>
            {
                logger.Debug("Configuring the context");

                // Register global objects.
                // FIXME: Potential security issue. Users can access .Type in script which allows to create any object and
                // thus run arbitrary code on the server.
                foreach (var entry in registeredGlobalObjects)
                    engine.GlobalObject.SetProperty(entry.Key, entry.Value, null, true, V8PropertyAttributes.Locked);

                logger.Debug("Calling context callbacks");

                // Invoke new context handlers.
                newContextHandlers.ForEach(handler => handler(context));

                logger.Debug("Executing serverScript");

                // Execute server script.
                engine.Execute(serverScript);
            };
        }

        private Dictionary<Guid, V8Engine> entityContexts = new Dictionary<Guid, V8Engine>();

        private Dictionary<string, object> registeredGlobalObjects = new Dictionary<string, object>();
        private List<Action<IJSContext>> newContextHandlers = new List<Action<IJSContext>>();

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}
