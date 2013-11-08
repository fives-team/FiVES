using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ValidPlugin1
{
    public class ValidPlugin1Initializer : IPluginInitializer
    {
        public string GetName()
        {
            return "ValidPlugin1";
        }

        public List<string> GetDependencies()
        {
            return new List<string>();
        }

        public void Initialize()
        {
        }
    }
}
