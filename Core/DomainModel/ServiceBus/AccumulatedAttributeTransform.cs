using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;

namespace FIVESServiceBus
{
    /// <summary>
    /// Stores all Attribute Transformations that are performed during a service chain computation in the service bus.
    /// It maintains a list of all recent intermediate results. Provides a function to query the latest actual
    /// attribute value of a transformed entity.
    /// </summary>
    public class AccumulatedAttributeTransform : EventArgs
    {
        public AccumulatedAttributeTransform(Entity entity,
            Dictionary<string, Dictionary<string, object>> accumulatedTransforms)
        {
            this.accumulatedTransforms = accumulatedTransforms;
            this.entity = entity;
        }

        /// <summary>
        /// Adds a transformation for an attribute to the accumulated transformation. If the same attribute was changed
        /// before, its value is replaced by the new one. If it was not in the list before, it is added to the list
        /// of transformations
        /// </summary>
        /// <param name="componentName">Component of which attribute should be changed</param>
        /// <param name="attributeName">Changed Attribute</param>
        /// <param name="newValue">New value of the attribute</param>
        public void AddAttributeTransformation(string componentName, string attributeName, object newValue)
        {
            if (!accumulatedTransforms.ContainsKey(componentName))
                accumulatedTransforms.Add(componentName, new Dictionary<string, object>());
            if (!accumulatedTransforms[componentName].ContainsKey(attributeName))
                accumulatedTransforms[componentName].Add(attributeName, newValue);
            else
                accumulatedTransforms[componentName][attributeName] = newValue;
        }

        /// <summary>
        /// Retrieves the current value of an attribute of the entity that is undergoing transformation. If it
        /// was transformed before, and thus is present in the accumulated transforms, this recent value is
        /// returned. Otherwise, the value contained in the ECA
        /// </summary>
        /// <param name="componentName">Component that contains the requested attribute</param>
        /// <param name="attributeName">Name of the requested attribute</param>
        /// <returns>Current attribute value</returns>
        public object CurrentAttributeValue(string componentName, string attributeName)
        {
            if (accumulatedTransforms.ContainsKey(componentName)
                && accumulatedTransforms[componentName].ContainsKey(attributeName))
            {
                return accumulatedTransforms[componentName][attributeName];
            }
            else
            {
                return entity[componentName][attributeName].Value;
            }
        }

        /// <summary>
        /// Registry of all attribute transformations that were performed so far
        /// </summary>
        public Dictionary<string, Dictionary<string, object>> AccumulatedTransformations
        {
            get
            {
                return accumulatedTransforms;
            }
        }

        /// <summary>
        /// Entity that is undergoing transformation
        /// </summary>
        public Entity Entity
        {
            get { return entity; }
        }

        private Entity entity;
        private Dictionary<string, Dictionary<string, object>> accumulatedTransforms;
    }
}
