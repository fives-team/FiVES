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
using SINFONI;
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
        public EntityCollection()
        {

        }

        public void Add(Entity entity)
        {
            lock(entities)
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
            bool didRemoveItem = false;

            lock (entities)
                didRemoveItem = entities.Remove(item.Guid);

            if (didRemoveItem)
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

        /// <summary>
        /// Returns true of an entity with a given guid is present in the collection.
        /// </summary>
        /// <param name="guid">Guid of the searched entity.</param>
        /// <returns>True of an entity with a given guid is present in the collection, false otherwise.</returns>
        public bool ContainsEntity(Guid guid)
        {
            return entities.ContainsKey(guid);
        }

        // Needed by persistence plugin.
        internal EntityCollection(ICollection<Entity> entityCollection)
        {
            foreach (Entity entity in entityCollection)
            {
                entities.Add(entity.Guid, entity);
            }
        }

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
