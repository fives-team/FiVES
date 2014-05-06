// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using NLog;
using System.Collections.Generic;

namespace NativeClient
{
    /// <summary>
    /// Handles client authentication.
    /// </summary>
    class Authenticator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NativeClient.Authenticator"/> class. The Communication must
        /// be connected.
        /// </summary>
        /// <param name="Communicator">The Communicator instance to be used to communicate with the server.</param>
        public Authenticator(Communicator communicator)
        {
            if (!communicator.IsConnected)
                throw new ArgumentException("Communicator must be connected when passed to Authenticator constructor.");

            Communicator = communicator;
            QueryAuthInterface();
        }

        /// <summary>
        /// Occurs when the client is authenticated.
        /// </summary>
        public event EventHandler Authenticated;

        void QueryAuthInterface()
        {
            int callID = Communicator.Call("kiara.implements", new List<string> { "auth" });
            Communicator.AddReplyHandler(callID, HandleQueryAuthInterfaceReply);
        }

        void HandleQueryAuthInterfaceReply(CallReply reply)
        {
            if (!reply.Success)
                Logger.Fatal("Failed to request auth service: {0}", reply.Message);

            List<bool> retValue = reply.RetValue.ToObject<List<bool>>();
            if (!retValue[0])
                Logger.Fatal("No auth service.");

            int callID = Communicator.Call("auth.login", GenerateRandomLogin(), "");
            Communicator.AddReplyHandler(callID, HandleAuthorizationReply);
        }

        void HandleAuthorizationReply(CallReply reply)
        {
            if (!reply.Success)
                Logger.Fatal("Failed on authentication: {0}", reply.Message);

            bool success = reply.RetValue.ToObject<bool>();
            if (!success)
                Logger.Fatal("Incorrect login/password", reply.Message);

            if (Authenticated != null)
                Authenticated(this, new EventArgs());
        }

        string GenerateRandomLogin()
        {
            var randomizer = new Random();
            return "user" + randomizer.Next();
        }

        Communicator Communicator;

        static Logger Logger = LogManager.GetCurrentClassLogger();
    }
}

