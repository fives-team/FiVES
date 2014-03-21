using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptingPlugin
{
    class LoggerScriptingInterface
    {
        public void debug(string message)
        {
            logger.Debug(message);
        }

        public void info(string message)
        {
            logger.Info(message);
        }

        public void warn(string message)
        {
            logger.Warn(message);
        }

        public void error(string message)
        {
            logger.Error(message);
        }

        public void fatal(string message)
        {
            logger.Fatal(message);
        }

        private Logger logger = LogManager.GetCurrentClassLogger();
    }
}
