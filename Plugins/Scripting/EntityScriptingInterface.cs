using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8.Net;

namespace ScriptingPlugin
{
    class EntityScriptingInterface
    {
        public static void InitializeEntityObject(object sender, NewContextCreatedArgs args)
        {
            var entityObj = new EntityScriptingInterface(args.Entity);
            args.Context.CreateGlobalObject("entity", entityObj);
        }

        public EntityScriptingInterface(Entity aEntity)
        {
            entity = aEntity;
        }

        public ComponentScriptingInterface this[string componentName]
        {
            get
            {
                try
                {
                    return new ComponentScriptingInterface(entity[componentName]);
                }
                catch (ComponentAccessException)
                {
                    // TODO: throw exception in the script
                    return null;
                }
            }
        }

        Entity entity;
    }
}
