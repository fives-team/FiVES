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
                {"startServersideAnimation", (Action<string, string, float, float>)StartServersideAnimation},
                {"stopServersideAnimation", (Action<string, string>)StopServersideAnimation},
                {"startClientsideAnimation", (Action<string, string>)StartClientsideAnimation},
                {"stopClientsideAnimation",(Action<string, string>)StopClientsideAnimation},
                {"notifyAboutClientsideAnimationStart", (Action<Connection, Action<string, string>>)ReceiveAnimationStartTrigger},
                {"notifyAboutClientsideAnimationStop", (Action<Connection, Action<string, string>>)ReceiveAnimationStopTrigger}
            });
        }

        private void StartServersideAnimation(string entityGuid, string name, float startFrame, float endFrame)
        {
            if (!manager.IsPlaying(entityGuid))
            {
                Animation newAnimation = new Animation(startFrame, endFrame);
                manager.StartAnimation(entityGuid, newAnimation);
            }
        }

        private void StopServersideAnimation(string entityGuid, string name)
        {
            manager.StopAnimation(entityGuid);
        }

        private void StartClientsideAnimation(string entityGuid, string animationName)
        {
            lock (animationStartCallbacks)
            {
                foreach (KeyValuePair<Connection, Action<string, string>> registeredCallback in animationStartCallbacks)
                {
                    var callback = registeredCallback.Value;
                    callback(entityGuid, animationName);
                }
            }
        }

        private void StopClientsideAnimation(string entityGuid, string animationName)
        {
            lock (animationStopCallbacks)
            {
                foreach (KeyValuePair<Connection, Action<string, string>> registeredCallback in animationStopCallbacks)
                {
                    var callback = registeredCallback.Value;
                    callback(entityGuid, animationName);
                }
            }
        }

        private void ReceiveAnimationStartTrigger(Connection clientConnection, Action<string, string> callback)
        {
            lock (animationStartCallbacks)
            {
                if (!animationStartCallbacks.ContainsKey(clientConnection))
                    animationStartCallbacks.Add(clientConnection, callback);
            }
        }

        private void ReceiveAnimationStopTrigger(Connection clientConnection, Action<string, string> callback)
        {
            lock (animationStopCallbacks)
            {
                if (!animationStopCallbacks.ContainsKey(clientConnection))
                    animationStopCallbacks.Add(clientConnection, callback);
            }
        }

        Dictionary<Connection, Action<string, string>> animationStartCallbacks = new Dictionary<Connection, Action<string, string>>();
        Dictionary<Connection, Action<string, string>> animationStopCallbacks = new Dictionary<Connection, Action<string, string>>();
        AnimationManager manager;
    }
}
