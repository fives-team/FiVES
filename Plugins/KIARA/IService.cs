using System;
using System.Collections.Generic;

namespace KIARAPlugin
{
    public interface IService
    {
        /// <summary>
        /// Returns the associated context for this service.
        /// </summary>
        /// <value>The associated context.</value>
        Context Context { get; }

        Delegate this[string name] { set; }
    }
}

