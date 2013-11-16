using KIARAPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScalabilityPlugin
{
    class SyncNode
    {
        public SyncNode(Connection connection)
        {
            Connection = connection;
            Connection["getActorID"]()
                .OnSuccess<string>(HandleActorIDResponse)
                .OnFailure(HandleActorIDFailure)
                .Wait();
        }

        public Connection Connection { get; private set; }

        public Guid RemoteActorID { get; private set; }

        private void HandleActorIDFailure()
        {
            throw new Exception("Failed to retrieve remote ActorID");
        }

        private void HandleActorIDResponse(string actorID)
        {
            RemoteActorID = Guid.Parse(actorID);
        }
    }
}
