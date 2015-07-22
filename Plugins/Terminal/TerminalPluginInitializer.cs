// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using FIVES;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerminalPlugin
{
    /// <summary>
    /// This is Terminal plugin initializer.
    /// </summary>
    public class TerminalPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get
            {
                return "Terminal";
            }
        }

        public List<string> PluginDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        public List<string> ComponentDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        public void Initialize()
        {
            Terminal.Instance = new Terminal();
            Commands.Instance = new Commands(Application.Controller);

            // Replace console target with custom target that will interleave logs with terminal.
            ConsoleTarget consoleTarget = LogManager.Configuration.FindTargetByName("console") as ConsoleTarget;
            if (consoleTarget != null)
                ReplaceNLogConsoleLogger(consoleTarget);
        }

        public void Shutdown()
        {
        }

        private void ReplaceNLogConsoleLogger(ConsoleTarget consoleTarget)
        {
            // Construct new methodCall target.
            MethodCallTarget methodCallTarget = new MethodCallTarget();
            methodCallTarget.ClassName = typeof(TerminalPluginInitializer).AssemblyQualifiedName;
            methodCallTarget.MethodName = "RedirectLogMessage";
            methodCallTarget.Parameters.Add(new MethodCallParameter(consoleTarget.Layout));

            // Add new methodCall target into the configuration.
            LogManager.Configuration.AddTarget("terminalLogger", methodCallTarget);

            // Reconfigure the logging rules using console target to use new methodCall target.
            foreach (LoggingRule rule in LogManager.Configuration.LoggingRules)
            {
                if (rule.Targets.Contains(consoleTarget))
                {
                    rule.Targets.Remove(consoleTarget);
                    rule.Targets.Add(methodCallTarget);
                }
            }

            // Reconfigure existing loggers.
            LogManager.ReconfigExistingLoggers();
        }

        public static void RedirectLogMessage(string message)
        {
            Terminal.Instance.WriteLine(message);
        }
    }
}
