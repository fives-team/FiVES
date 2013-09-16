
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Events;

namespace FIVES
{
    #region Testing
    public interface IEntityRegistry
    {
        Entity GetEntity(Guid guid);
        HashSet<Guid> GetAllGUIDs();
    }
    #endregion

    /// <summary>
    /// Manages entities in the database
    /// </summary>
    public class EntityRegistry : IEntityRegistry
    {
        public readonly static EntityRegistry Instance = new EntityRegistry();

        public delegate void EntityAdded (Object sender, EntityAddedOrRemovedEventArgs e);
        public delegate void EntityRemoved (Object sender, EntityAddedOrRemovedEventArgs e);

        public event EntityAdded OnEntityAdded;
        public event EntityRemoved OnEntityRemoved;

        /// <summary>
        /// Adds a new entity to the database.
        /// </summary>
        /// <param name="entity">Entity that is to be added to the registry</param>
        public void AddEntity(Entity entity)
        {
            Entities[entity.Guid] = entity;
            if (OnEntityAdded != null)
                OnEntityAdded(this, new EntityAddedOrRemovedEventArgs(entity.Guid));
        }

        /// <summary>
        ///  Removes a given <paramref name="entity"/>. Returns false if such entity was not found.
        /// </summary>
        /// <returns><c>true</c>, if entity was removed, <c>false</c> otherwise.</returns>
        /// <param name="entity">Entity that should be removed.</param>
        public bool RemoveEntity(Entity entity)
        {
            return RemoveEntity(entity.Guid);
        }

        /// <summary>
        ///  Removes an entity with a given <b>guid</b>. Returns false if such entity was not found.
        /// </summary>
        /// <returns><c>true</c>, if entity was removed, <c>false</c> otherwise.</returns>
        /// <param name="guid">GUID of the entity that should be removed</param>
        public bool RemoveEntity(Guid guid)
        {
            if (Entities.ContainsKey(guid)) {
                if (OnEntityRemoved != null)
                    OnEntityRemoved(this, new EntityAddedOrRemovedEventArgs(guid));
                Entities.Remove(guid);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns entity by its <b>guid</b> or null if no such entity is found.
        /// </summary>
        /// <returns>The entity by GUID.</returns>
        /// <param name="guid">GUID.</param>
        public Entity GetEntity(Guid guid)
        {
            if (Entities.ContainsKey(guid))
                return Entities[guid];
            return null;
        }

        /// <summary>
        /// Returns an entity by its <paramref name="guid"/> or null if no such entity is found.
        /// </summary>
        /// <returns>The entity.</returns>
        /// <param name="guid">GUID as a string.</param>
        public Entity GetEntity(string guid)
        {
            return GetEntity(new Guid(guid));
        }

        /// <summary>
        ///  Returns a list of all entities' GUIDs.
        /// </summary>
        /// <returns>The all GUI ds.</returns>
        public HashSet<Guid> GetAllGUIDs()
        {
            HashSet<Guid> res = new HashSet<Guid>();
            foreach (Guid guid in Entities.Keys)
                res.Add(guid);
            return res;
        }

        // Users should not construct EntityRegistry on their own, but use EntityRegistry.Instance instead.
        internal EntityRegistry() {
            OnEntityAdded += new EntityAdded (HandleOnNewEntity);
            OnEntityRemoved += new EntityRemoved (HandleOnEntityRemoved);
        }

        private void HandleOnNewEntity(Object sender, EntityAddedOrRemovedEventArgs e) {
            Debug.WriteLine ("Added Entity " + e.elementId);
        }

        private void HandleOnEntityRemoved(Object sender, EntityAddedOrRemovedEventArgs e) {
            Debug.WriteLine ("Removed Entity " + e.elementId);
        }
        /// <summary>
        /// All registered entities, indexed by their Guids
        /// </summary>
        private IDictionary<Guid, Entity> Entities = new Dictionary<Guid, Entity>();

        /// <summary>
        /// The registry GUID.
        /// </summary>
        public readonly Guid RegistryGuid = new Guid("0f5f96c5-30cb-4b4f-b06f-e5efd257a3c9");
    }
}

