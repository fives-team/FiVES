using System;

namespace KIARA
{
    public abstract class Service
    {
        /// <summary>
        /// Returns the associated context for this service.
        /// </summary>
        /// <value>The associated context.</value>
        public Context context { get; private set; }

        protected Service(Context aContext)
        {
            context = aContext;
        }
    }
}

