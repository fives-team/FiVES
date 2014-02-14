using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScalabilityPlugin
{
    /// <summary>
    /// This is used to store the sync state of the entity including all of its attributes. This class is also used to
    /// transfer updates to other sync nodes - necessary information to resolve update conflicts for attributes is
    /// stored with each attribute. There is no need to provide a separate timestamp for the entire entity, because the
    /// only modification operations that may occur are removal and addition, which do not conflict with each other -
    /// addition always introduces an entity with a different guid, thus removal cannot happen before addition has been
    /// propagated to respective node. Note that this does not consider the possibility of updates swithing order on
    /// the network, therefore the order must be guaranteed. We may need to change the synchronization algorithms if we
    /// decide to allow loops in the sync graph or if we will start using unreliable out-of-order protocol such as UDP.
    /// Currently we use reliable in-order WebSocket protocol and allow no loops in the sync graph.
    /// </summary>
    class EntitySyncInfo
    {
        /// <summary>
        /// Convenience operator for getting or setting attribute sync info. Allows to use square brackets without the
        /// need to write the Components property name: entitySyncInfo["component"]. If a component sync info doesn't
        /// exist when accessed via the getter - it will be automatically created. This allows to avoid checking
        /// whether component exists first.
        /// </summary>
        /// <param name="componentName">Component name.</param>
        /// <returns>Relevant sync info.</returns>
        public ComponentSyncInfo this[string componentName]
        {
            get
            {
                if (!Components.ContainsKey(componentName))
                    Components[componentName] = new ComponentSyncInfo();
                return Components[componentName];
            }
            set
            {
                Components[componentName] = value;
            }
        }

        /// <summary>
        /// A collection of entity's component sync info. Key is component name.
        /// </summary>
        public Dictionary<string, ComponentSyncInfo> Components = new Dictionary<string, ComponentSyncInfo>();
    }
}
