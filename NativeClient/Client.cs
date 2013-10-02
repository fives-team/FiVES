using System;
using WebSocket4Net;
using NLog;
using System.Configuration;
using SuperSocket.ClientEngine;

namespace NativeClient
{
    class MainClass
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private static WebSocket websocket;

        public static void Main(string[] args)
        {
            Logger.Info("Reading configuration");

            string serverURI = null;

            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            try {
                serverURI = ConfigurationManager.AppSettings["ServerURI"].ToString();
            } catch (ConfigurationErrorsException) {
                Logger.Error("Configuration is missing or corrupt.");
                return;
            }

            Logger.Info("Connecting to the server");

            websocket = new WebSocket(serverURI);
            websocket.Opened += new EventHandler(websocket_Opened);
            websocket.Error += new EventHandler<ErrorEventArgs>(websocket_Error);
            websocket.Closed += new EventHandler(websocket_Closed);
            websocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(websocket_MessageReceived);
            websocket.Open();

            Logger.Info("Main thread complete");
        }

        private static void websocket_Opened(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void websocket_Error(object sender, ErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void websocket_Closed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
