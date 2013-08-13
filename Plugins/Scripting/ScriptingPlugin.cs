using System;
using FIVES;
using System.Collections.Generic;

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
            // TODO: Initialize scripting engine
        }

        #endregion

        private Dictionary<string, object> registeredGlobalObjects = new Dictionary<string, object>();
    }
}

