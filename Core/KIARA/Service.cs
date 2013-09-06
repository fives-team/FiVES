using System;

namespace KIARA
{
    public abstract class Service
    {
        /// <summary>
        /// Gets the UUID of the service.
        /// </summary>
        /// <value>The UUID of the service.</value>
        public Guid uuid { get; private set; }

        /// <summary>
        /// Returns the associated context for this service.
        /// </summary>
        /// <value>The associated context.</value>
        public Context context { get; private set; }

        protected Service(Context aContext)
        {
            uuid = Guid.NewGuid();
            context = aContext;
        }
    }
}

