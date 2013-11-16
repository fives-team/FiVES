using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
{
    /// <summary>
    /// Represents a collection of top-level entities in the world. When new entities are added to a World instance,
    /// they are automatically removed from their parent if any and their Parent property is set to null.
    /// </summary>
    public sealed class World : EntityCollection
    {
        public static World Instance = new World();
    }
}
