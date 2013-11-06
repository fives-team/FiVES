using System;
using FIVES;
using System.Collections.Generic;
using KIARA;
using Events;
using Renderable;
using Math;

namespace Editing
{
    /// <summary>
    /// Plugin that allows changing the world by the users.
    /// </summary>
    public class EditingPlugin : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string GetName()
        {
            return "Editing";
        }

        public List<string> GetDependencies()
        {
            return new List<string>() { "Location" };
        }

        public void Initialize()
        {
            PluginManager.Instance.AddPluginLoadedHandler("ClientManager", RegisterEditingAPI);
        }

        #endregion

        /// <summary>
        /// Creates an entity at x, y and z.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        public string CreateEntityAt(Vector position)
        {
            Entity entity = new Entity();
            entity["position"]["x"] = position.x;
            entity["position"]["y"] = position.y;
            entity["position"]["z"] = position.z;
            EntityRegistry.Instance.AddEntity(entity);
            return entity.Guid.ToString ();
        }

        public string CreateMeshEntity(Vector position, Quat orientation, Vector scale, MeshResource mesh)
        {
            Entity entity = new Entity();
            entity["position"]["x"] = position.x;
            entity["position"]["y"] = position.y;
            entity["position"]["z"] = position.z;

            entity["orientation"]["x"] = orientation.x;
            entity["orientation"]["y"] = orientation.y;
            entity["orientation"]["z"] = orientation.z;
            entity["orientation"]["w"] = orientation.w;

            entity["scale"]["x"] = scale.x;
            entity["scale"]["y"] = scale.y;
            entity["scale"]["z"] = scale.z;

            entity["meshResource"]["uri"] = mesh.meshURI;
            entity["meshResource"]["visible"] = true;
            EntityRegistry.Instance.AddEntity(entity);
            return entity.Guid.ToString ();
        }

        /// <summary>
        /// Registers editing APIs with the ClientManager plugin.
        /// </summary>
        private void RegisterEditingAPI() {
            var clientManager = ServiceFactory.DiscoverByName("clientmanager", ContextFactory.GetContext("inter-plugin"));
            clientManager.OnConnected += delegate(Connection connection) {
                connection["registerClientService"]("editing", true, new Dictionary<string, Delegate> {
                    {"createEntityAt", (Func<Vector, string>)CreateEntityAt},
                    {"createMeshEntity",(Func<Vector, Quat, Vector,MeshResource, string>)CreateMeshEntity}
                });
            };
        }
    }
}
