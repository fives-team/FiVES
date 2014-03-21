using System;

namespace ScriptingPlugin
{
    public class Scripting
    {
        public static IScripting Instance;

        public static void RegisterGlobalObject(string name, object csObject)
        {
            Instance.RegisterGlobalObject(name, csObject);
        }

        public static event EventHandler<NewContextCreatedArgs> NewContextCreated
        {
            add
            {
                Instance.NewContextCreated += value;
            }
            remove
            {
                Instance.NewContextCreated -= value;
            }
        }
    }
}
