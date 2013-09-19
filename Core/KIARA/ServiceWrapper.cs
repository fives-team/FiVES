using System;

namespace KIARA
{
    public class ServiceWrapper : Service
    {
        /// <summary>
        /// Occurs when connection is established. New handlers are also invoked, even if connection have been 
        /// established before they were added.
        /// </summary>
        public delegate void Connected(Connection connection);
        private event Connected InternalOnConnected;
        public event Connected OnConnected {
            add {
                if (connection == null)
                    InternalOnConnected += value;
                else
                    value(connection);
            }
            remove {
                if (connection == null)
                    InternalOnConnected -= value;
            }
        }

        internal void HandleConnected(Connection aConnection)
        {
            connection = aConnection;

            if (InternalOnConnected != null)
                InternalOnConnected(aConnection);
        }

        internal ServiceWrapper(Context context) : base(context) {}

        private Connection connection = null;
    }
}

