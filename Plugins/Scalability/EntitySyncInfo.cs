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
        /// A collection of entity's attributes.
        /// </summary>
        public Dictionary<AttributePath, AttributeSyncInfo> Attributes =
            new Dictionary<AttributePath, AttributeSyncInfo>();
    }
}
