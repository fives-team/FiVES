using System;
using FIVES;
using System.Collections.Generic;
using KIARA;

namespace Scripting
{
    public class ScriptingPlugin : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string getName()
        {
            return "Scripting";
        }

        public List<string> getDependencies()
        {
            return new List<string>();
        }

        public void initialize()
        {
            // Provide scripting API to other plugins.
            pluginContext.startServer(pluginConfig, registerPluginMethods);

            // Register 'scripting' component.
            ComponentLayout layout = new ComponentLayout();
            layout["ownerScript"] = AttributeType.STRING;
            layout["serverScript"] = AttributeType.STRING;
            layout["clientScript"] = AttributeType.STRING;
            ComponentRegistry.Instance.defineComponent("scripting", pluginGUID, layout);

            // TODO: register for changes to the scripting component via event system (once implemented)
            // TODO: register for creation of new entities and parse their scripting component (if present)
        }

        #endregion

        private void registerGlobalObject(string name, object csObject)
        {
            if (registeredGlobalObjects.ContainsKey(name))
                throw new Exception("The object with " + name + " is already registered");
            registeredGlobalObjects[name] = csObject;
        }

        private void addNewContextHandler(Action<IJavaScriptContext> handler)
        {
            newContextHandlers.Add(handler);
        }

        private void registerPluginMethods(Connection conn)
        {
            conn.registerFuncImplementation("registerGlobalObject", (Action<string, object>)registerGlobalObject);
            conn.registerFuncImplementation("addNewContextHandler",
                                            (Action<Action<IJavaScriptContext>>)addNewContextHandler);
        }

        private Dictionary<string, object> registeredGlobalObjects = new Dictionary<string, object>();
        private List<Action<IJavaScriptContext>> newContextHandlers = new List<Action<IJavaScriptContext>>();
        private Context pluginContext = new Context();

        // {
        //   'info': 'ClientSyncPlugin',
        //   'idlContent': '...',
        //   'servers': [{
        //     'services': '*',
        //     'protocol': {
        //       'name': 'direct-call',
        //       'id': 'scripting',
        //     },
        //   }],
        // }
        private readonly string pluginConfig = "data:text/json;base64,ew0KICAnaW5mbyc6ICdDbGllbnRTeW5jUGx1Z2luJywNCiA" +
            "gJ2lkbENvbnRlbnQnOiAnLi4uJywNCiAgJ3NlcnZlcnMnOiBbew0KICAgICdzZXJ2aWNlcyc6ICcqJywNCiAgICAncHJvdG9jb2wnOiB" +
            "7DQogICAgICAnbmFtZSc6ICdkaXJlY3QtY2FsbCcsDQogICAgICAnaWQnOiAnc2NyaXB0aW5nJywNCiAgICB9LA0KICB9XSwNCn0=";

        private readonly Guid pluginGUID = new Guid("90dd4c50-f09d-11e2-b778-0800200c9a66");
    }
}

