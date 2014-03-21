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

            // Initialize entity object in the new contexts.
            NewContextCreated += EntityScriptingInterface.InitializeEntityObject;

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

            // Create global object for existing contexts.
            foreach (var contextPair in entityContexts)
                contextPair.Value.CreateGlobalObject(name, csObject);
        }

        /// <summary>
        /// Triggered when a new scripting context has been created, which is done for each entity which has a server
        /// script. This can be used to perform non-trivial initialization of the scripting API.
        /// </summary>
        public event EventHandler<NewContextCreatedArgs> NewContextCreated
        {
            add
            {
                newContextCreated += value;

                // Trigger new context handler for all existing contexts.
                foreach (var contextPair in entityContexts)
                    value(this, new NewContextCreatedArgs(contextPair.Key, contextPair.Value));
            }
            remove
            {
                newContextCreated -= value;
            }
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
            entityContexts.Remove(entity);

            logger.Debug("Entered initEntityContext with entity [{0}]", entity.Guid);

            string serverScript = (string)entity["scripting"]["serverScript"];
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

            logger.Debug("Creating context wrapper");

            // This object should be used to assign event handlers, e.g.
            // script.onNewObject = function (newObject) {...}
            var context = new V8NetContext(engine, serverScript, (string)entity["scripting"]["serverScriptDeps"]);

            logger.Debug("Adding context to the list");

            entityContexts.Add(entity, context);

            logger.Debug("Creating script object");

            context.Execute("script = {}");

            logger.Debug("About to enter context scope");

            logger.Debug("Configuring the context");

            // Register global objects.
            // FIXME: Potential security issue. Users can access .Type in script which allows to create any object
            // and thus run arbitrary code on the server.
            foreach (var entry in registeredGlobalObjects)
                context.CreateGlobalObject(entry.Key, entry.Value);

            logger.Debug("Calling context callbacks");

            // Invoke new context handlers.
            if (newContextCreated != null)
                newContextCreated(this, new NewContextCreatedArgs(entity, context));
        }

        private Dictionary<Entity, V8NetContext> entityContexts = new Dictionary<Entity, V8NetContext>();

        private Dictionary<string, object> registeredGlobalObjects = new Dictionary<string, object>();
        private event EventHandler<NewContextCreatedArgs> newContextCreated;

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}
