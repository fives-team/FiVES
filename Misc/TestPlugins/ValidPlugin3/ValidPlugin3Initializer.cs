using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ValidPlugin3
{
    public class ValidPlugin3Initializer : IPluginInitializer
    {
        public string Name
        {
            get 
            {
                return "ValidPlugin3";
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
                return new List<string> { "testComponentForValidPlugin3" };
            }
        }

        public void Initialize()
        {
        }
    }
}
