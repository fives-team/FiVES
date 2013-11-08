using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewCorePrototype
{
    /// <summary>
    /// Represents a collection of entities.
    /// </summary>
    public class EntityCollection : ICollection<Entity>
    {
        public void Add(Entity entity)
        {
            entities.Add(entity);
            HandleAdded(entity);
        }

        public void Clear()
        {
            List<Entity> removedEntities = new List<Entity>();
            removedEntities.AddRange(entities);
            entities.Clear();
            foreach (Entity entity in removedEntities)
                HandleRemoved(entity);
        }

        public bool Contains(Entity item)
        {
            return entities.Contains(item);
        }

        public void CopyTo(Entity[] array, int arrayIndex)
        {
            entities.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return entities.Count; }
        }

        public bool Remove(Entity item)
        {
            if (entities.Remove(item))
            {
                HandleRemoved(item);
                return true;
            }

            return false;
        }

        public IEnumerator<Entity> GetEnumerator()
        {
            return entities.GetEnumerator();
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
        public event EventHandler<EntityEventArgs> EntityAdded;

        /// <summary>
        /// Raised when an entity has been removed.
        /// </summary>
        public event EventHandler<EntityEventArgs> EntityRemoved;

        internal EntityCollection()
        {
        }

        private void HandleAdded(Entity entity)
        {
            if (EntityAdded != null)
                EntityAdded(this, new EntityEventArgs(entity));
        }

        private void HandleRemoved(Entity entity)
        {
            if (EntityRemoved != null)
                EntityRemoved(this, new EntityEventArgs(entity));
        }

        private List<Entity> entities = new List<Entity>();
    }
}
