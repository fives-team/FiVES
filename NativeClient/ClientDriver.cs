using System;
using WebSocket4Net;
using NLog;
using SuperSocket.ClientEngine;
using System.Collections.Generic;

namespace NativeClient
{
    public class ClientDriver
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private static WebSocket socket;

        public ClientDriver(string serverURI)
        {
            Logger.Info("Connecting to the server");
            socket = new WebSocket(serverURI);
            socket.Opened += new EventHandler(HandleSocketOpened);
            socket.Error += new EventHandler<ErrorEventArgs>(HandleSocketError);
            socket.Closed += new EventHandler(HandleSocketClosed);
            socket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(HandleSocketMessage);
        }

        public void SimulateClient()
        {
            var machine = CreateStateMachine();
            machine.Run();
        }

        StateMachine CreateStateMachine()
        {
            StateMachine machine = new StateMachine("connect");

            // Connect to the world.
            machine.AddStateAction("connect", new DelegateAction(delegate { socket.Open(); }));
            machine.AddErrorTransition("connect", "Disconnected before connected",
                                       new EventCondition(h => socket.Closed += h));
            machine.AddStateTransition("connect", "implements1", new EventCondition(h => socket.Opened += h));

            // Call kiara.implements.
            var implementsCall = new CallFuncAction(socket, "kiara.implements", new List<string> { "auth.login" });
            implementsCall.ExpectedValue = new bool[] { true };
            machine.AddStateAction("implements1", implementsCall);
            machine.AddErrorTransition("implements1", "Failed to get auth interface", implementsCall.FailureCondition);
            machine.AddStateTransition("implements1", "wait", implementsCall.SuccessCondition);

            // Wait 5 seconds
            machine.AddStateAction("wait", new LogAction(Logger, "Waiting for 5 seconds..."));
            machine.AddStateTransition("wait", "auth", new DelayCondition(5000));

            // Call auth.login.
            var loginCall = new CallFuncAction(socket, "auth.login", GenerateRandomLogin(), "");
            machine.AddStateAction("auth", loginCall);
            machine.AddErrorTransition("auth", "Failed to login", loginCall.FailureCondition);
            machine.AddStateTransition("auth", "complete", loginCall.SuccessCondition);

            // Print success message and disconnect.
            machine.AddStateAction("complete", new LogAction(Logger, "Logged into the world. Disconnecting..."));
            machine.AddStateAction("complete", new DelegateAction(delegate { socket.Close(); }));
            machine.AddStateTransition("complete", machine.FinalState, new EventCondition(h => socket.Closed += h));

            // Connection error may happen anytime.
            var connectionErrorHandler = machine.GetUniversalErrorHandler("Connection error");
            socket.Error += (sender, e) => connectionErrorHandler(sender, e);

            return machine;
        }

        string GenerateRandomLogin()
        {
            var randomizer = new Random();
            return "user" + randomizer.Next();
        }

        void HandleSocketOpened(object sender, EventArgs e)
        {
            Logger.Info("Connected to the server");
        }

        void HandleSocketError(object sender, ErrorEventArgs e)
        {
            Logger.ErrorException("Error in WebSocket connection", e.Exception);
        }

        void HandleSocketClosed(object sender, EventArgs e)
        {
            Logger.Info("Connection closed");
        }

        void HandleSocketMessage(object sender, MessageReceivedEventArgs e)
        {
            Logger.Debug("Received: {0}", e.Message);
        }
    }
}

