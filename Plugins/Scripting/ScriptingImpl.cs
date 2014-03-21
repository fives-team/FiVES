using FIVES;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8.Net;

namespace ScriptingPlugin
{
    class ScriptingImpl : IScripting
    {
        public ScriptingImpl()
        {
            // Global object to enable logging from the scripts.
            RegisterGlobalObject("logger", new LoggerScriptingInterface());

            World.Instance.AddedEntity += HandleOnEntityAdded;

            // Register event handlers for all entities that were created before this plugin was loaded.
            foreach (var entity in World.Instance)
                HandleOnEntityAdded(World.Instance, new EntityEventArgs(entity));
        }

        /// <summary>
        /// Registers a new global object in the script context. The fields and methods of the new object correspond to
        /// the passed C# object.
        /// </summary>
        /// <param name="name">Name of the global object.</param>
        /// <param name="csObject">Corresponding C# object.</param>
        public void RegisterGlobalObject(string name, object csObject)
        {
            if (registeredGlobalObjects.ContainsKey(name))
                throw new Exception("The object with " + name + " is already registered");
            registeredGlobalObjects[name] = csObject;

            // Initialize scripts whose dependencies have been satisfied
            List<Entity> initializedEntities = new List<Entity>();
            foreach (var deferredScriptPair in deferredScripts)
            {
                deferredScriptPair.Value.Remove(name);
                if (deferredScriptPair.Value.Count == 0)
                {
                    InitEntityContext(deferredScriptPair.Key);
                    initializedEntities.Add(deferredScriptPair.Key);
                }
            }

            initializedEntities.ForEach(e => deferredScripts.Remove(e));
        }

        /// <summary>
        /// Adds a new handler which is invoked each time a new context is create, which happens every time a new
        /// scripted object is discovered.
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

            string serverScript = (string)entity["scripting"]["serverScript"];
            if (serverScript == null)
                return;

            // Check for the script dependencies (global objects).
            string serverScriptDeps = (string)entity["scripting"]["serverScriptDeps"];
            if (serverScriptDeps != null)
            {
                string[] deps = serverScriptDeps.Split(' ', ',');
                List<string> unsatisfiedDeps = deps.ToList().FindAll(s => !registeredGlobalObjects.ContainsKey(s));
                if (unsatisfiedDeps.Count > 0)
                {
                    deferredScripts.Add(entity, unsatisfiedDeps);
                    return;
                }
            }
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

            // This object should be used to assign event handlers, e.g.
            // script.onNewObject = function (newObject) {...}
            var context = new V8NetContext(engine);

            logger.Debug("Creating script object");

            context.Execute("script = {}");

            logger.Debug("About to enter context scope");

            engine.WithContextScope = () =>
            {
                logger.Debug("Configuring the context");

                // Register global objects.
                // FIXME: Potential security issue. Users can access .Type in script which allows to create any object
                // and thus run arbitrary code on the server.
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

        // List of deferred scripts. These have some of their dependencies (global objects) unsatisfied.
        private Dictionary<Entity, List<string>> deferredScripts = new Dictionary<Entity, List<string>>();

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}
