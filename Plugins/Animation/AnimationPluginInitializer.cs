using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientManagerPlugin;
using FIVES;
using KIARAPlugin;

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
            get { return new List<string> {"EventLoop", "ClientManager"}; }
        }

        public List<string> ComponentDependencies
        {
            get { return new List<string>(); }
        }

        public void Initialize()
        {
            RegisterComponents();
            RegisterClientServices();
            manager = new AnimationManager();
        }

        private void RegisterComponents()
        {
            ComponentDefinition animationComponent = new ComponentDefinition("animation");
            animationComponent.AddAttribute<float>("keyframe", 0f);
            ComponentRegistry.Instance.Register(animationComponent);
        }

        private void RegisterClientServices()
        {
            ClientManager.Instance.RegisterClientService("animation", false, new Dictionary<string, Delegate>
            {
                {"startServersideAnimation", (Action<string, float, float>)HandleStartAnimation},
                {"stopServersideAnimation", (Action<string>)HandleStopAnimation},
            });
        }

        private void HandleStartAnimation(string entityGuid, float startFrame, float endFrame)
        {
            if (!manager.IsPlaying(entityGuid))
            {
                Animation newAnimation = new Animation(startFrame, endFrame);
                manager.StartAnimation(entityGuid, newAnimation);
            }
        }

        private void HandleStopAnimation(string entityGuid)
        {
            manager.StopAnimation(entityGuid);
        }

        AnimationManager manager;
    }
}
