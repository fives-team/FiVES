using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ValidPlugin2
{
    public class ValidPlugin2Initializer : IPluginInitializer
    {
        public string Name
        {
            get
            {
                return "ValidPlugin2";
            }
        }

        public List<string> RequiredPlugins
        {
            get
            {
                return new List<string> { "ValidPlugin1" };
            }
        }

        public List<string> RequiredComponents
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
