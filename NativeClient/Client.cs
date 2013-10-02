using System;
using WebSocket4Net;
using NLog;

namespace NativeClient
{
    class MainClass
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            Logger.Info("Reading configuration");

            string serverURI = null;

            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            try {
                serverURI = ConfigurationManager.AppSettings["ServerURI"].ToString();
            } catch (ConfigurationErrorsException) {
                logger.Error("Configuration is missing or corrupt.");
                return;
            }

            Logger.Info("Reading configuration");

            WebSocket connection = new WebSocket(serverURI);
            websocket.Opened += new EventHandler(websocket_Opened);
            websocket.Error += new EventHandler<ErrorEventArgs>(websocket_Error);
            websocket.Closed += new EventHandler(websocket_Closed);
            websocket.MessageReceived += new EventHandler(websocket_MessageReceived);
            websocket.Open();
        }

        private void websocket_Opened(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void websocket_Error(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void websocket_Closed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void websocket_MessageReceived(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
