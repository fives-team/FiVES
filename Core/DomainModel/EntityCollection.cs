using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
{
    /// <summary>
    /// Represents a collection of entities.
    /// </summary>
    public class EntityCollection : ICollection<Entity>
    {
        public void Add(Entity entity)
        {
            entities.Add(entity.Guid, entity);
            HandleAdded(entity);
        }

        public void Clear()
        {
            List<Entity> removedEntities = new List<Entity>();
            removedEntities.AddRange(entities.Values);
            entities.Clear();
            foreach (Entity entity in removedEntities)
                HandleRemoved(entity);
        }

        public bool Contains(Entity item)
        {
            return entities.ContainsKey(item.Guid);
        }

        public void CopyTo(Entity[] array, int arrayIndex)
        {
            entities.Values.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return entities.Count; }
        }

        public bool Remove(Entity item)
        {
            if (entities.Remove(item.Guid))
            {
                HandleRemoved(item);
                return true;
            }

            return false;
        }

        public IEnumerator<Entity> GetEnumerator()
        {
            return entities.Values.GetEnumerator();
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return entities.GetEnumerator();
        }

        /// <summary>
        /// Raised when a new entity has been added.
        /// </summary>
        public event EventHandler<EntityEventArgs> AddedEntity;

        /// <summary>
        /// Raised when an entity has been removed.
        /// </summary>
        public event EventHandler<EntityEventArgs> RemovedEntity;

        /// <summary>
        /// Finds an entity by its Guid as string. Throws EntityNotFoundException if entity is not found.
        /// </summary>
        /// <param name="guid">String representation of the unique identifier.</param>
        /// <returns>An entity.</returns>
        public Entity FindEntity(string guid)
        {
            Guid parsedGuid = Guid.Parse(guid);
            return FindEntity(parsedGuid);
        }

        /// <summary>
        /// Finds an entity by its Guid. Throws EntityNotFoundException if entity is not found.
        /// </summary>
        /// <param name="guid">Guid of the entity.</param>
        /// <returns>An entity.</returns>
        public Entity FindEntity(Guid guid)
        {
            if (!entities.ContainsKey(guid))
                throw new EntityNotFoundException("Entity with given guid is not found.");

            return entities[guid];
        }

        // Needed by persistence plugin.
        internal EntityCollection(ICollection<Entity> entityCollection)
        {
            foreach (Entity entity in entityCollection)
            {
                entities.Add(entity.Guid, entity);
            }
        }

        // Needed for testing.
        internal EntityCollection() { }

        private void HandleAdded(Entity entity)
        {
            if (AddedEntity != null)
                AddedEntity(this, new EntityEventArgs(entity));
        }

        private void HandleRemoved(Entity entity)
        {
            if (RemovedEntity != null)
                RemovedEntity(this, new EntityEventArgs(entity));
        }

        protected Dictionary<Guid, Entity> entities = new Dictionary<Guid, Entity>();
    }
}
