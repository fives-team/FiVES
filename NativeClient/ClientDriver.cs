using System;
using WebSocket4Net;
using NLog;
using SuperSocket.ClientEngine;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace NativeClient
{
    public class ClientDriver
    {
        public ClientDriver(string serverURI, bool enableMovement, bool enableRotation)
        {
            Logger.Info("Connecting to the server");

            communicator = new Communicator(serverURI);
            communicator.Connected += HandleConnected;
            communicator.Disconnected += HandleDisconnected;

            EnableMovement = enableMovement;
            EnableRotation = enableRotation;
        }

        void HandleDisconnected (object sender, EventArgs e)
        {
            Environment.Exit(-1);
        }

        void HandleConnected(object sender, EventArgs e)
        {
            Logger.Info("Authenticating as a client");

            Authenticator authenticator = new Authenticator(communicator);
            authenticator.Authenticated += HandleAuthenticated;
        }

        void HandleAuthenticated(object sender, AuthenticatedEventArgs e)
        {
            Logger.Info("Loading world");

            worldManager = new WorldManager(communicator, e.SessionKey);
            worldManager.Loaded += HandleWorldLoaded;
        }

        void HandleWorldLoaded(object sender, EventArgs e)
        {
            if (EnableMovement)
            {
                Logger.Info("Starting to move entities");
                worldManager.StartMovingAllEntities();
            }

            if (EnableRotation)
            {
                Logger.Info("Starting to rotate entities");
                worldManager.StartRotatingAllEntities();
            }
        }


        bool EnableMovement;
        bool EnableRotation;

        Communicator communicator;
        WorldManager worldManager;

        static Logger Logger = LogManager.GetCurrentClassLogger();
    }
}