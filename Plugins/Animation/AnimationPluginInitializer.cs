using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FIVES;

namespace AnimationPlugin
{
    public class AnimationPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get { return "Animation"; }
        }

        public List<string> PluginDependencies
        {
            get { return new List<string> {"EventLoop"}; }
        }

        public List<string> ComponentDependencies
        {
            get { return new List<string>(); }
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        private void RegisterComponents()
        {
            ComponentDefinition animationComponent = new ComponentDefinition();
            animationComponent.AddAttribute<float>("keyframe", 0);
        }
    }
}
