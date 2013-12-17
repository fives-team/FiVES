using System;
using WebSocket4Net;
using NLog;
using SuperSocket.ClientEngine;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Configuration;
using System.Collections.Specialized;

namespace NativeClient
{
    public class ClientDriver
    {
        /// <summary>
        /// Configures the client driver from a set of name-value pairs. Ingores invalid or missing values.
        /// </summary>
        /// <param name="settings">Name-value pairs for settings.</param>
        public void Configure(NameValueCollection settings)
        {
            ReadStringSetting(settings, "ServerURI", ref ServerURI);
            ReadStringSetting(settings, "ServerHost", ref ServerHost);
            ReadIntSetting(settings, "ServerPort", ref ServerPort);
            ReadBoolSetting(settings, "EnableMovement", ref EnableMovement);
            ReadBoolSetting(settings, "EnableRotation", ref EnableRotation);
            ReadIntSetting(settings, "NumEntitiesToGenerate", ref NumEntitiesToGenerate);
            ReadIntSetting(settings, "ActionDelayMs", ref ActionDelayMs);
        }

        /// <summary>
        /// Starts the simulation of the client.
        /// </summary>
        public void StartSimulation()
        {
            logger.Info("Connecting to the server");

            communicator = new Communicator();
            communicator.Connected += HandleConnected;
            communicator.Disconnected += HandleDisconnected;
            if (ServerURI != null)
                communicator.OpenConnection(ServerURI);
            else
                communicator.OpenConnection(ServerHost, ServerPort);
        }

        void ReadStringSetting(NameValueCollection settings, string settingName, ref string value)
        {
            if (settings[settingName] != null)
                value = settings[settingName];
        }

        void ReadBoolSetting(NameValueCollection settings, string settingName, ref bool value)
        {
            if (settings[settingName] != null)
                value = settings[settingName] == "true";
        }

        void ReadIntSetting(NameValueCollection settings, string settingName, ref int value)
        {
            if (settings[settingName] != null)
            {
                int parsedValue;
                if (Int32.TryParse(settings[settingName], out parsedValue))
                    value = parsedValue;
            }
        }

        void HandleDisconnected (object sender, EventArgs e)
        {
            Environment.Exit(-1);
        }

        void HandleConnected(object sender, EventArgs e)
        {
            logger.Info("Estimating time delay");

            TimeDelayEstimator estimator = new TimeDelayEstimator(communicator);
            estimator.TimeDelayEstimated += HandleTimeDelayEstimated;
            estimator.StartEstimating(100);
        }

        void HandleTimeDelayEstimated (object sender, TimeDelayEventArgs e)
        {
            logger.Info("Delay to server is " + e.TimeDelayMs);

            logger.Info("Authenticating as a client");

            Authenticator authenticator = new Authenticator(communicator);
            authenticator.Authenticated += HandleAuthenticated;
        }

        void HandleAuthenticated(object sender, AuthenticatedEventArgs e)
        {
            logger.Info("Loading world");

            worldManager = new WorldManager(communicator, e.SessionKey);
            worldManager.Loaded += HandleWorldLoaded;

            StartCreatingEntities();
        }

        void StartCreatingEntities()
        {
            for (int i = 0; i < NumEntitiesToGenerate; i++)
                worldManager.CreateEntity();
        }

        void HandleWorldLoaded(object sender, EventArgs e)
        {
            if (EnableMovement)
            {
                logger.Info("Starting to move entities");
                StartMovingLocallyCreatedEntities();
            }

            if (EnableRotation)
            {
                logger.Info("Starting to rotate entities");
                StartRotatingLocallyCreatedEntities();
            }
        }

        void StartMovingLocallyCreatedEntities()
        {
            new Thread(MoveLocallyCreatedEntities).Start();
        }

        void StartRotatingLocallyCreatedEntities()
        {
            new Thread(RotateLocallyCreatedEntities).Start();
        }

        void MoveEntity(EntityInfo info)
        {
            info.Position.x = Timestamps.FloatMilliseconds;
            communicator.Call("location.updatePosition", worldManager.SessionKey, info.Guid, info.Position,
                              Timestamps.UnixTimestamp);
        }

        void MoveLocallyCreatedEntities()
        {
            while (true)
            {
                lock (worldManager.Entities)
                    worldManager.Entities.FindAll(e => e.IsLocallyCreated).ForEach(MoveEntity);
                Thread.Sleep(ActionDelayMs);
            }
        }

        void RotateEntity(EntityInfo info)
        {
            AxisAngle aa = new AxisAngle();
            aa.FromQuaternion(info.Orientation);
            aa.Angle += 0.1;

            if (aa.Angle > 2 * Math.PI)
                aa.Angle = 0;

            info.Orientation = aa.ToQuaternion();

            communicator.Call("location.updateOrientation", worldManager.SessionKey, info.Guid, info.Orientation,
                              Timestamps.UnixTimestamp);
        }

        void RotateLocallyCreatedEntities()
        {
            while (true)
            {
                lock (worldManager.Entities)
                    worldManager.Entities.FindAll(e => e.IsLocallyCreated).ForEach(RotateEntity);
                Thread.Sleep(ActionDelayMs);
            }
        }

        string ServerURI = null;
        string ServerHost = "127.0.0.1";
        int ServerPort = 34837;
        bool EnableMovement = true;
        bool EnableRotation = true;
        int NumEntitiesToGenerate = 1;
        int ActionDelayMs = 250;

        Communicator communicator;
        WorldManager worldManager;

        static Logger logger = LogManager.GetCurrentClassLogger();
    }
}
