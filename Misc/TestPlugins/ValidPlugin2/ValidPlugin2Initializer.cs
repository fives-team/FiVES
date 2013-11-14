using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ValidPlugin2
{
    public class ValidPlugin2Initializer : IPluginInitializer
    {
        public string GetName()
        {
            return "ValidPlugin2";
        }

        public List<string> GetDependencies()
        {
            return new List<string> { "ValidPlugin1" };
        }

        public void Initialize()
        {
        }
    }
}
