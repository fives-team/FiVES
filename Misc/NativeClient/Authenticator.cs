using System;
using NLog;
using System.Collections.Generic;

namespace NativeClient
{
    /// <summary>
    /// Authenticated event arguments.
    /// </summary>
    class AuthenticatedEventArgs : EventArgs
    {
        public AuthenticatedEventArgs(string sessionKey)
        {
            SessionKey = sessionKey;
        }

        /// <summary>
        /// Session key associated with the authenticated client.
        /// </summary>
        /// <value>The session key.</value>
        public string SessionKey { get; private set; }
    }

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
        public event EventHandler<AuthenticatedEventArgs> Authenticated;

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

            var sessionKey = reply.RetValue.ToObject<string>();
            if (new Guid(sessionKey) == Guid.Empty)
                Logger.Fatal("Incorrect login/password", reply.Message);

            if (Authenticated != null)
                Authenticated(this, new AuthenticatedEventArgs(sessionKey));
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

