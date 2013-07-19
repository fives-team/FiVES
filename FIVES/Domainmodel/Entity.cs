using System;
using System.Collections.Generic;

namespace FIVES
{
    public class EntityHasNoChildrenException : Exception
    {
        public EntityHasNoChildrenException() {}
        public EntityHasNoChildrenException(string message) {}
    }

    public class Entity
    {
        private Dictionary<string, Component> components = new Dictionary<string, Component>();
        public Entity parent { get; set; }
        private List<Entity> children  = new List<Entity> ();

        public Entity ()
        {

        }

        public Component this [string index]
        {
            get {
                return this.components [index];
            }
            set {
                this.components [index] = value;
            }
        }

        public bool addChildNode(Entity childEntity)
        {
            if (this.children.Contains (childEntity))
                return false;

            childEntity.parent = this;
            this.children.Add (childEntity);
            return true;
        }

        public List<Entity> getAllChildren()
        {
            return children;
        }

        public Entity getFirstChild()
        {
            if(children.Count == 0)
                throw(new EntityHasNoChildrenException("List of children for Entity is empty"));
            return children[0];
        }

        public Entity getLastChild()
        {
            if(children.Count == 0)
                throw(new EntityHasNoChildrenException("List of children for Entity is empty"));
            return children [children.Count -1];
        }
    }
}

