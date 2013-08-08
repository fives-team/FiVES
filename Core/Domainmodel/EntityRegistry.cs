
using System;
using System.Collections.Generic;

namespace FIVES
{
    // Manages entities in the database.
    public class EntityRegistry
    {
        public readonly static EntityRegistry Instance = new EntityRegistry();

        // Adds a new entity to the database. Returns GUID assigned to the entity.
        public Guid addEntity(Entity entity)
        {
            Guid newGUID = Guid.NewGuid();
            entity.Guid = newGUID;
            this.addEntityWithGUID(entity);
            return newGUID;
        }

        {
            entities[entity.Guid] = entity;
        }
        // Removes an entity with a given |guid|. Returns false if such entity was not found.
        public bool removeEntity(Guid guid)
        {
            if (entities.ContainsKey(guid)) {
                entities.Remove(guid);
                return true;
            }

            return false;
        }

        // Returns entity by its |guid| or null if no such entity is found.
        public Entity getEntityByGuid(Guid guid)
        {
            if (entities.ContainsKey(guid))
                return entities[guid];
            return null;
        }

        // Returns a list of all entities' GUIDs.
        public List<Guid> getAllGUIDs()
        {
            List<Guid> res = new List<Guid>();
            foreach (Guid guid in entities.Keys)
                res.Add(guid);
            return res;
        }

        // Users should not construct EntityRegistry on their own, but use EntityRegistry.Instance instead.
        internal EntityRegistry() {}

        private IDictionary<Guid, Entity> entities = new Dictionary<Guid, Entity>();
        public readonly Guid RegistryGuid = new Guid("0f5f96c5-30cb-4b4f-b06f-e5efd257a3c9");
    }
}

