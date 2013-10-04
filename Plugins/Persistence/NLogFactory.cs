using System;
using System.Collections.Specialized;
using System.Configuration;
using NLog;
using NHibernate;

namespace Persistence
{
    public class NLogFactory : ILoggerFactory
    {
        #region ILoggerFactory Members

        public IInternalLogger LoggerFor(System.Type type)
        {
            return new NLogLogger();
        }

        public IInternalLogger LoggerFor(string keyName)
        {
            return new NLogLogger();
        }

        #endregion
    }

    public class NLogLogger : IInternalLogger
    {
        private static readonly NLog.Logger log = LogManager.GetCurrentClassLogger();

        private static readonly string ConfigSectionName = "nhibernate_nlog";

        private static readonly string DebugKey = "debug";
        private static readonly string ErrorKey = "error";
        private static readonly string FatalKey = "fatal";
        private static readonly string InfoKey = "info";
        private static readonly string WarnKey = "warn";

        public NLogLogger()
        {
            InitProperties();
        }

        #region IInternalLogger Members

        #region Properties

        public bool IsDebugEnabled { get; private set; }

        public bool IsErrorEnabled { get; private set; }

        public bool IsFatalEnabled { get; private set; }

        public bool IsInfoEnabled { get; private set; }

        public bool IsWarnEnabled { get; private set; }

        #endregion

        #region IInternalLogger Methods

        public void Debug(object message, Exception exception)
        {
            if (IsDebugEnabled)
                log.DebugException(message.ToString(), exception);
        }

        public void Debug(object message)
        {
            if (IsDebugEnabled)
                log.Debug(message.ToString());
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (IsDebugEnabled)
                log.Debug(String.Format(format, args));
        }

        public void Error(object message, Exception exception)
        {
            if (IsErrorEnabled)
                log.ErrorException(message.ToString(), exception);
        }

        public void Error(object message)
        {
            if (IsErrorEnabled)
                log.Error(message.ToString());
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (IsErrorEnabled)
                log.Error(String.Format(format, args));
        }

        public void Fatal(object message, Exception exception)
        {
            if (IsFatalEnabled)
                log.FatalException(message.ToString(), exception);
        }

        public void Fatal(object message)
        {
            if (IsFatalEnabled)
                log.Fatal(message.ToString());
        }

        public void Info(object message, Exception exception)
        {
            if (IsInfoEnabled)
                log.InfoException(message.ToString(), exception);
        }

        public void Info(object message)
        {
            if (IsInfoEnabled)
                log.Info(message.ToString());
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (IsInfoEnabled)
                log.Info(String.Format(format, args));
        }

        public void Warn(object message, Exception exception)
        {
            if (IsWarnEnabled)
                log.WarnException(message.ToString(), exception);
        }

        public void Warn(object message)
        {
            if (IsWarnEnabled)
                log.Warn(message.ToString());
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (IsWarnEnabled)
                log.Warn(String.Format(format, args));
        }

        #endregion

        #endregion

        #region Private methods

        private void InitProperties()
        {
            IsErrorEnabled = true;
            IsFatalEnabled = true;
            IsWarnEnabled = true;

            //System.Diagnostics.Debug.WriteLine("Finding section");
            var section = ConfigurationManager.GetSection(ConfigSectionName) as NameValueCollection;

            //System.Diagnostics.Debug.WriteLine(section != null ? "Section found" : "Section not found");

            if (section != null)
            {
                bool flag = false;

                if (section[DebugKey] != null && Boolean.TryParse(section[DebugKey], out flag))
                    IsDebugEnabled = flag;
                if (section[ErrorKey] != null && Boolean.TryParse(section[ErrorKey], out flag))
                    IsErrorEnabled = flag;
                if (section[FatalKey] != null && Boolean.TryParse(section[FatalKey], out flag))
                    IsFatalEnabled = flag;
                if (section[InfoKey] != null && Boolean.TryParse(section[InfoKey], out flag))
                    IsInfoEnabled = flag;
                if (section[WarnKey] != null && Boolean.TryParse(section[WarnKey], out flag))
                    IsWarnEnabled = flag;
            }
        }

        #endregion
    }
}