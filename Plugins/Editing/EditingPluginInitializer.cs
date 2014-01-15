using System;
using FIVES;
using System.Collections.Generic;
using RenderablePlugin;
using ClientManagerPlugin;

namespace EditingNamespace
{
    /// <summary>
    /// Plugin that allows changing the world by the users.
    /// </summary>
    public class EditingPluginInitializer : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string Name
        {
            get
            {
                return "Editing";
            }
        }

        public List<string> PluginDependencies
        {
            get
            {
                return new List<string> { "Renderable" };
            }
        }

        public List<string> ComponentDependencies
        {
            get
            {
                return new List<string> { "position", "orientation", "scale", "meshResource" };
            }
        }

        public void Initialize()
        {
            PluginManager.Instance.AddPluginLoadedHandler("ClientManager", RegisterEditingAPI);
        }

        public void Shutdown()
        {
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
            World.Instance.Add(entity);
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

            entity["meshResource"]["uri"] = mesh.uri;
            entity["meshResource"]["visible"] = true;
            World.Instance.Add(entity);
            return entity.Guid.ToString ();
        }

        /// <summary>
        /// Registers editing APIs with the ClientManager plugin.
        /// </summary>
        private void RegisterEditingAPI() {
            ClientManager.Instance.RegisterClientService("editing", true, new Dictionary<string, Delegate> {
                {"createEntityAt", (Func<Vector, string>)CreateEntityAt},
                {"createMeshEntity",(Func<Vector, Quat, Vector, MeshResource, string>)CreateMeshEntity}
            });
        }
    }
}
