// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Specialized;
using System.Configuration;
using NLog;
using NHibernate;

namespace PersistencePlugin
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

            string persistenceConfigPath = this.GetType().Assembly.Location;
            Configuration config = ConfigurationManager.OpenExeConfiguration(persistenceConfigPath);
            var section = config.AppSettings.Settings;

            //System.Diagnostics.Debug.WriteLine(section != null ? "Section found" : "Section not found");

            if (section != null)
            {
                bool flag = false;

                if (section[DebugKey] != null && Boolean.TryParse(section[DebugKey].Value, out flag))
                    IsDebugEnabled = flag;
                if (section[ErrorKey] != null && Boolean.TryParse(section[ErrorKey].Value, out flag))
                    IsErrorEnabled = flag;
                if (section[FatalKey] != null && Boolean.TryParse(section[FatalKey].Value, out flag))
                    IsFatalEnabled = flag;
                if (section[InfoKey] != null && Boolean.TryParse(section[InfoKey].Value, out flag))
                    IsInfoEnabled = flag;
                if (section[WarnKey] != null && Boolean.TryParse(section[WarnKey].Value, out flag))
                    IsWarnEnabled = flag;
            }
        }

        #endregion
    }
}