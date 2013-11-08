using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
{
    /// <summary>
    /// Represents a collection of top-level entities in the world.
    /// </summary>
    public sealed class World : EntityCollection
    {
        public static World Instance = new World();
    }
}
