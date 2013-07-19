
using System;
using System.Collections.Generic;

namespace FIVES
{
    // Manages entities in the database.
    public class EntityRegistry
    {
        // Adds a new entity to the database. Returns GUID assigned to the entity.
        public static Guid addEntity(Entity entity)
        {
            Guid newGUID = Guid.NewGuid();
            entities[newGUID] = entity;
            return newGUID;
        }

        // Removes an entity with a given |guid|. Returns false if such entity was not found.
        public static bool removeEntity(Guid guid)
        {
            if (entities.ContainsKey(guid)) {
                entities.Remove(guid);
                return true;
            }

            return false;
        }

        // Returns entity by its |guid| or null if no such entity is found.
        public static Entity getEntityByGuid(Guid guid)
        {
            if (entities.ContainsKey(guid))
                return entities[guid];
            return null;
        }

        // Returns a list of all entities' GUIDs.
        public static List<Guid> getAllGUIDs()
        {
            List<Guid> res = new List<Guid>();
            foreach (Guid guid in entities.Keys)
                res.Add(guid);
            return res;
        }

        private static Dictionary<Guid, Entity> entities = new Dictionary<Guid, Entity>();
    }
}

