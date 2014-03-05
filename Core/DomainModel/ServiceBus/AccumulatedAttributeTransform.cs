using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;

namespace ServiceBusPlugin
{
    public class AccumulatedAttributeTransform : EventArgs
    {
        public AccumulatedAttributeTransform(Entity entity, Dictionary<string, Dictionary<string, object>> accumulatedTransforms)
        {
            this.accumulatedTransforms = accumulatedTransforms;
            this.entity = entity;
        }

        public void AddAttributeTransformation(string componentName, string attributeName, object newValue)
        {
            if (!accumulatedTransforms.ContainsKey(componentName))
                accumulatedTransforms.Add(componentName, new Dictionary<string, object>());
            if (!accumulatedTransforms[componentName].ContainsKey(attributeName))
                accumulatedTransforms[componentName].Add(attributeName, newValue);
            else
                accumulatedTransforms[componentName][attributeName] = newValue;
        }

        public Dictionary<string, Dictionary<string, object>> AccumulatedTransformations
        {
            get
            {
                return accumulatedTransforms;
            }
        }

        public Entity Entity
        {
            get { return entity; }
        }

        private Entity entity;
        private Dictionary<string, Dictionary<string, object>> accumulatedTransforms;
    }
}
