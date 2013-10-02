using System;
using NLog;

namespace NativeClient
{
    public class LogAction : IStateAction
    {
        public LogAction(Logger logger, string message) {
            Logger = logger;
            Message = message;
        }

        #region IStateAction implementation

        public void Execute()
        {
            Logger.Info(Message);
        }

        #endregion

        Logger Logger;
        string Message;
    }
}

