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
            socket.Opened += new EventHandler(LogSocketOpened);
            socket.Error += new EventHandler<ErrorEventArgs>(LogSocketError);
            socket.Closed += new EventHandler(LogSocketClosed);
            socket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(LogSocketMessage);
        }

        public void SimulateClient()
        {
            var machine = CreateStateMachine();
            machine.Run();
        }

        StateMachine CreateStateMachine()
        {
            StateMachine machine = new StateMachine("connect");

            // Connect to the server.
            machine.AddStateAction("connect", new DelegateAction(delegate { socket.Open(); }));
            machine.AddErrorTransition("connect", "Disconnected before connected",
                                       new EventCondition(h => socket.Closed += h));
            machine.AddStateTransition("connect", "implements1", new EventCondition(h => socket.Opened += h));

            // Call kiara.implements.
            var implementsCall = new CallFuncAction(socket, "kiara.implements", new List<string> { "auth.login" });
            implementsCall.SetExpectedValue(new bool[] { true });
            machine.AddStateAction("implements", implementsCall);
            machine.AddErrorTransition("implements", "Failed to get auth interface", implementsCall.FailureCondition);
            machine.AddStateTransition("implements", "wait", implementsCall.SuccessCondition);

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

        void LogSocketOpened(object sender, EventArgs e)
        {
            Logger.Info("Connected to the server");
        }

        void LogSocketError(object sender, ErrorEventArgs e)
        {
            Logger.ErrorException("Connection error", e.Exception);
        }

        void LogSocketClosed(object sender, EventArgs e)
        {
            Logger.Info("Connection closed");
        }

        void LogSocketMessage(object sender, MessageReceivedEventArgs e)
        {
            Logger.Debug("Received: {0}", e.Message);
        }
    }
}

