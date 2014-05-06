// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using FIVES;
using System.Collections.Generic;
using ClientManagerPlugin;

namespace EditingNamespace
{
#pragma warning disable 649
    struct MeshResource
    {
        public string uri;
        public bool visible;
    }
#pragma warning restore 649

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
        string CreateEntityAt(Vector position)
        {
            Entity entity = new Entity();
            entity["location"]["position"] = position;
            World.Instance.Add(entity);
            return entity.Guid.ToString ();
        }

        string CreateMeshEntity(Vector position, Quat orientation, Vector scale, MeshResource mesh)
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
                {"createMeshEntity", (Func<Vector, Quat, Vector, MeshResource, string>)CreateMeshEntity}
            });
        }
    }
}
