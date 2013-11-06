using System;
using System.Collections.Generic;

namespace KIARAPlugin
{
    public class ContextFactory
    {
        public static Context GetContext(string name)
        {
            if (contextCache.ContainsKey(name))
                return contextCache[name];
            return contextCache[name] = new Context();
        }

        private static Dictionary<string, Context> contextCache = new Dictionary<string, Context>();
    }
}

