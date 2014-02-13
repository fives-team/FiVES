using System;
using FIVES;
using System.Collections.Generic;
using ClientManagerPlugin;

namespace EditingNamespace
{
    struct MeshResource
    {
        public string uri;
        public bool visible;
    }


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
                return new List<string>();
            }
        }

        public List<string> ComponentDependencies
        {
            get
            {
                return new List<string> { "location", "mesh" };
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
        /// Creates an entity at the given position.
        /// </summary>
        /// <param name="position">Given position.</param>
        public string CreateEntityAt(Vector position)
        {
            Entity entity = new Entity();
            entity["location"]["position"] = position;
            World.Instance.Add(entity);
            return entity.Guid.ToString ();
        }

        public string CreateMeshEntity(Vector position, Quat orientation, Vector scale, MeshResource mesh)
        {
            Entity entity = new Entity();
            entity["location"]["position"] = position;
            entity["location"]["orientation"] = orientation;
            entity["mesh"]["scale"] = scale;
            entity["mesh"]["uri"] = mesh.uri;
            entity["mesh"]["visible"] = mesh.visible;
            World.Instance.Add(entity);
            return entity.Guid.ToString ();
        }

        /// <summary>
        /// Registers editing APIs with the ClientManager plugin.
        /// </summary>
        private void RegisterEditingAPI() {
            ClientManager.Instance.RegisterClientService("editing", true, new Dictionary<string, Delegate> {
                {"createEntityAt", (Func<Vector, string>)CreateEntityAt},
                {"createMeshEntity", (Func<Vector, Quat, Vector, string, bool, string>)CreateMeshEntity}
            });
        }
    }
}
