using System;
using FIVES;
using System.Collections.Generic;
using KIARA;
using Events;
using V8.Net;

namespace Scripting
{
    /// <summary>
    /// Scripting plugin.
    /// </summary>
    public class ScriptingPlugin : IPluginInitializer
    {
        #region IPluginInitializer implementation

        /// <summary>
        /// Returns the name of the plugin.
        /// </summary>
        /// <returns>The name of the plugin.</returns>
        public string getName()
        {
            return "Scripting";
        }

        /// <summary>
        /// Returns the list of names of the plugins that this plugin depends on.
        /// </summary>
        /// <returns>The list of names of the plugins that this plugin depends on.</returns>
        public List<string> getDependencies()
        {
            return new List<string>();
        }

        /// <summary>
        /// Initializes the plugin. This method will be called by the plugin manager when all dependency plugins have
        /// been loaded.
        /// </summary>
        public void initialize()
        {
            // Provide scripting API to other plugins.
            pluginContext.startServer(pluginConfig, registerPluginMethods);

            // Register 'scripting' component.
            ComponentLayout layout = new ComponentLayout();
            layout[ownerScriptAttributeName] = typeof(string);
            layout[serverScriptAttributeName] = typeof(string);
            layout[clientScriptAttributeName] = typeof(string);
            ComponentRegistry.Instance.defineComponent(scriptingComponentName, pluginGUID, layout);

            EntityRegistry.Instance.OnEntityAdded += handleOnEntityAdded;
        }

        #endregion

        private const string scriptingComponentName = "scripting";
        private const string ownerScriptAttributeName = "ownerScript";
        private const string serverScriptAttributeName = "serverScript";
        private const string clientScriptAttributeName = "clientScript";

        private void handleOnEntityAdded(object sender, EntityAddedOrRemovedEventArgs e)
        {
            Entity newEntity = EntityRegistry.Instance.getEntity(e.elementId);
            // FIXME: This is not very efficient. Would be nice to be triggered only when anything in "scripting" 
            // component is changed, but without the need to create one. Essentially we need an event 
            // OnComponentCreated.
            newEntity.OnAttributeInComponentChanged += delegate(object sender2, AttributeInComponentEventArgs e2) {
                if (e2.componentName == scriptingComponentName)
                    initEntityContext(newEntity);
            };
            initEntityContext(newEntity);
        }


        private void initEntityContext(Entity entity)
        {
            // Remove previous context if any.
            entityContexts.Remove(entity.Guid);

            string serverScript = (string)entity[scriptingComponentName][serverScriptAttributeName];
            if (serverScript == null)
                return;

            var engine = new V8Engine();
            entityContexts.Add(entity.Guid, engine);

            // This object should be used to assign event handlers, e.g. script.onNewObject = function (newObject) {...}
            var context = new V8NetContext(engine);
            context.Execute("script = {}");

            // Register global objects.
            foreach (var entry in registeredGlobalObjects)
                engine.GlobalObject.SetProperty(entry.Key, entry.Value, null, true, V8PropertyAttributes.Locked);

            // Invoke new context handlers.
            newContextHandlers.ForEach(handler => handler(context));

            // Execute server script.
            engine.Execute(serverScript);
        }

        private void registerGlobalObject(string name, object csObject)
        {
            if (registeredGlobalObjects.ContainsKey(name))
                throw new Exception("The object with " + name + " is already registered");
            registeredGlobalObjects[name] = csObject;
        }

        private void addNewContextHandler(Action<IJSContext> handler)
        {
            newContextHandlers.Add(handler);
        }

        private void registerPluginMethods(Connection conn)
        {
            conn.registerFuncImplementation("registerGlobalObject", (Action<string, object>)registerGlobalObject);
            conn.registerFuncImplementation("addNewContextHandler",
                                            (Action<Action<IJSContext>>)addNewContextHandler);
        }

        private Dictionary<string, object> registeredGlobalObjects = new Dictionary<string, object>();
        private List<Action<IJSContext>> newContextHandlers = new List<Action<IJSContext>>();
        private Context pluginContext = new Context();

        private Dictionary<Guid, V8Engine> entityContexts = new Dictionary<Guid, V8Engine>();

        // {
        //   'info': 'ScriptingPlugin',
        //   'idlContent': '...',
        //   'servers': [{
        //     'services': '*',
        //     'protocol': {
        //       'name': 'direct-call',
        //       'id': 'scripting',
        //     },
        //   }],
        // }
        private readonly string pluginConfig = "data:text/json;base64,ew0KICAnaW5mbyc6ICdTY3JpcHRpbmdQbHVnaW4nLA0KICA" +
            "naWRsQ29udGVudCc6ICcuLi4nLA0KICAnc2VydmVycyc6IFt7DQogICAgJ3NlcnZpY2VzJzogJyonLA0KICAgICdwcm90b2NvbCc6IHs" +
            "NCiAgICAgICduYW1lJzogJ2RpcmVjdC1jYWxsJywNCiAgICAgICdpZCc6ICdzY3JpcHRpbmcnLA0KICAgIH0sDQogIH1dLA0KfQ==";

        private readonly Guid pluginGUID = new Guid("90dd4c50-f09d-11e2-b778-0800200c9a66");
    }
}

