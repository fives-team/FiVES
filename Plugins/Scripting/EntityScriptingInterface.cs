using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptingPlugin
{
    class EntityScriptingInterface
    {
        public static void InitializeEntityObject(object sender, NewContextCreatedArgs args)
        {
            var entityObj = new EntityScriptingInterface(args.Entity);
            args.Context.CreateGlobalObject("entity", entityObj);

            args.Context.RegisterTypeConstructors(typeof(Vector));
            args.Context.RegisterTypeConstructors(typeof(AxisAngle));
            args.Context.RegisterTypeConstructors(typeof(Quat));
        }

        public EntityScriptingInterface(Entity anEntity)
        {
            entity = anEntity;
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

        public string toString()
        {
            return "[entity " + entity.Guid + "]";
        }

        Entity entity;
    }
}
