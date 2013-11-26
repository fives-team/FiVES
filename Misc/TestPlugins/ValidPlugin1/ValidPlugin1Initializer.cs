using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ValidPlugin1
{
    public class ValidPlugin1Initializer : IPluginInitializer
    {
        public string Name
        {
            get
            {
                return "ValidPlugin1";
            }
        }

        public List<string> PluginDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        public List<string> ComponentDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        public void Initialize()
        {
        }
    }
}
