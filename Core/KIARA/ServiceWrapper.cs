using System;

namespace KIARA
{
    public class ServiceWrapper : Service
    {
        public delegate void Connected(Connection connection);
        public event Connected OnConnected;

        public FuncWrapper this[string name]
        {
            get
            {
                if (connection != null)
                    return connection.generateFuncWrapper(name);
                else
                    throw new Error(ErrorCode.CONNECTION_ERROR,
                                    "Connection is not established yet. Please use OnConnected event.");
            }
        }

        internal void HandleConnected(Connection aConnection)
        {
            connection = aConnection;

            if (OnConnected != null)
                OnConnected(aConnection);
        }

        internal ServiceWrapper(Context context) : base(context) {}

        private Connection connection;
    }
}

