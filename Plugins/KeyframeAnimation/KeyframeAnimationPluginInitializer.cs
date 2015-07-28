// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientManagerPlugin;
using FIVES;
using System.IO;
using SINFONI;

namespace KeyframeAnimationPlugin
{
    public class KeyframeAnimationPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get { return "KeyframeAnimation"; }
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
            RegisterToEvents();
            KeyframeAnimationManager.Instance = new KeyframeAnimationManager();
            KeyframeAnimationManager.Instance.Initialize(this);
        }

        public void Shutdown()
        {
        }

        internal void RegisterComponents()
        {
            ComponentDefinition animationComponent = new ComponentDefinition("animation");
            animationComponent.AddAttribute<string>("animationKeyframes");
            ComponentRegistry.Instance.Register(animationComponent);
        }

        private void RegisterToEvents()
        {
            ClientManager.Instance.NewClientConnected += new EventHandler<ClientConnectionEventArgs>(HandleClientConnected);
            ClientManager.Instance.ClientDisconnected += new EventHandler<ClientConnectionEventArgs>(HandleClientDisconnected);
        }

        private void RegisterClientServices()
        {
            string animationIdl = File.ReadAllText("keyframeAnimation.kiara");
            SINFONIPlugin.SINFONIServerManager.Instance.SinfoniServer.AmendIDL(animationIdl);
            ClientManager.Instance.RegisterClientService("animation", false, new Dictionary<string, Delegate>
            {
                {"startServersideAnimation",
                    (Action<string, string, float, float, int, float>)StartServersideAnimation},
                {"stopServersideAnimation",
                    (Action<string, string>)StopServersideAnimation},
                {"startClientsideAnimation",
                    (Action<string, string, float, float, int, float>)StartClientsideAnimation},
                {"stopClientsideAnimation",
                    (Action<string, string>)StopClientsideAnimation}
            });
        }

        /// <summary>
        /// SINFONI Service method handler that initiates a server side animation playback for an entity
        /// </summary>
        /// <param name="entityGuid">Guid of entity for which animation should be played</param>
        /// <param name="name">Name of animation that should be played</param>
        /// <param name="startFrame">Keyframe at which animation playback should start</param>
        /// <param name="endFrame">Keyframe at which animation playback should end</param>
        /// <param name="cycles">Number of cycles the animation should be played (-1 for infinite playback)</param>
        /// <param param name="speed">Speed in which animation should be played</param>
        internal void StartServersideAnimation(string entityGuid,
                                               string animationName,
                                               float startFrame, float endFrame,
                                               int cycles, float speed)
        {
            KeyframeAnimation newAnimation = new KeyframeAnimation(animationName, startFrame, endFrame, cycles, speed);
            KeyframeAnimationManager.Instance.StartAnimation(new Guid(entityGuid), newAnimation);
        }

        /// <summary>
        /// Handler that stops a server side animation playback for an entity
        /// </summary>
        /// <param name="entityGuid">Guid for which playback should be stopped</param>
        /// <param name="name">Name of animation for which playback should be stopped</param>
        internal void StopServersideAnimation(string entityGuid, string name)
        {
            KeyframeAnimationManager.Instance.StopAnimation(new Guid(entityGuid), name);
        }

        /// <summary>
        /// Invokes a message to start animations on clients' sides
        /// </summary>
        /// <param name="entityGuid">Guid of entity for which animation should be played</param>
        /// <param name="name">Name of animation that should be played</param>
        /// <param name="startFrame">Keyframe at which animation playback should start</param>
        /// <param name="endFrame">Keyframe at which animation playback should end</param>
        /// <param name="cycles">Number of cycles the animation should be played (-1 for infinite playback)</param>
        /// <param param name="speed">Speed in which animation should be played</param>
        internal void StartClientsideAnimation(string entityGuid,
                                              string animationName,
                                              float startFrame, float endFrame,
                                              int cycles, float speed)
        {
            lock (animationStartCallbacks)
            {
                foreach (ClientFunction animationStartTrigger in animationStartCallbacks.Values)
                {
                    animationStartTrigger(entityGuid, animationName, startFrame, endFrame, cycles, speed);
                }
            }
        }

        /// <summary>
        /// Invokes a message to stop animations on clients' sides
        /// </summary>
        /// <param name="entityGuid">Guid of entity for which animation should be played</param>
        /// <param name="name">Name of animation that should be played</param>
        internal void StopClientsideAnimation(string entityGuid, string animationName)
        {
            lock (animationStopCallbacks)
            {
                foreach (ClientFunction animationStopTrigger in animationStopCallbacks.Values)
                {
                    animationStopTrigger(entityGuid, animationName);
                }
            }
        }

        private void HandleClientConnected(object sender, ClientConnectionEventArgs e)
        {
            var clientConnection = e.ClientConnection;
            ClientFunction animationStartTrigger = clientConnection.GenerateClientFunction("animation", "receiveClientsideAnimationStart");
            ClientFunction animationStopTrigger = clientConnection.GenerateClientFunction("animation", "receiveClientsideAnimationStop");
            lock (animationStartCallbacks)
            {
                if (!animationStartCallbacks.ContainsKey(clientConnection))
                    animationStartCallbacks.Add(clientConnection, animationStartTrigger);
            }
            lock (animationStopCallbacks)
            {
                if (!animationStopCallbacks.ContainsKey(clientConnection))
                    animationStopCallbacks.Add(clientConnection, animationStopTrigger);
            }
        }

        private void HandleClientDisconnected(object sender, ClientConnectionEventArgs e)
        {
            var clientConnection = e.ClientConnection;
            lock(animationStartCallbacks)
            {
                if (animationStartCallbacks.ContainsKey(clientConnection))
                    animationStartCallbacks.Remove(clientConnection);
            }
            lock(animationStopCallbacks)
            {
                if (animationStopCallbacks.ContainsKey(clientConnection))
                    animationStopCallbacks.Remove(clientConnection);
            }
        }

        Dictionary<Connection, ClientFunction> animationStartCallbacks = new Dictionary<Connection, ClientFunction>();
        Dictionary<Connection, ClientFunction> animationStopCallbacks = new Dictionary<Connection, ClientFunction>();
    }
}
