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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace TerminalPlugin
{
    /// <summary>
    /// This class implements the terminal plugin API.
    /// </summary>
    public class Terminal
    {
        public static Terminal Instance;

        public Terminal()
        {
            RegisterCommand("help", "Shows help for a command or all commands if none specified.", false, ShowHelp,
                new List<string> { "h", "?" });

            if (!IsConsoleIORedirected())
            {
                Application.Controller.PluginsLoaded += HandlePluginsLoaded;
                Application.Controller.ControlTaken = true;
            }
            else
            {
                logger.Warn("Terminal plugin is disabled due to the redirected input stream.");
            }
        }

        /// <summary>
        /// Prints an empty line to the screen.
        /// </summary>
        public void WriteLine()
        {
            WriteLine("");
        }

        /// <summary>
        /// Prints a given line to the screen.
        /// </summary>
        public void WriteLine(string line)
        {
            lock (consoleLock)
            {
                StringBuilder commandLineClean = new StringBuilder();
                while (commandLineClean.Length < previousCommandLineLength)
                    commandLineClean.Append(' ');

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(commandLineClean);
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.WriteLine(line);

                UpdateCommandLineUnlocked();
            }
        }

        /// <summary>
        /// Registers a command for the terminal.
        /// </summary>
        /// <exception cref="ArgumentException">
        ///   Thrown when a command or any of its aliases are already registered.
        /// </exception>
        /// <param name="name">Name of the command.</param>
        /// <param name="helpText">Help text used for this command in the help command.</param>
        /// <param name="caseSensitive">True if name is case-sensitive.</param>
        /// <param name="aliases">An optional set of aliases.</param>
        /// <param name="handler">Handler to be invoked when command is executed.</param>
        public void RegisterCommand(string name, string helpText, bool caseSensitive, Action<string> handler,
                                    List<string> aliases = null)
        {
            if (name == null)
                throw new ArgumentException("Command can not be null", "name");

            if (handler == null)
                throw new ArgumentException("Handler can not be null", "handler");

            var info = new CommandInfo
            {
                Name = name,
                HelpText = helpText,
                CaseSensitive = caseSensitive,
                Handler = handler,
                Aliases = aliases ?? new List<string>()
            };

            lock (commands)
            {
                AddCommand(name, info);

                if (aliases != null)
                {
                    foreach (string alias in aliases)
                        AddCommand(alias, info);
                }
            }
        }

        bool IsConsoleIORedirected()
        {
            // http://stackoverflow.com/questions/3453220/how-to-detect-if-console-in-stdin-has-been-redirected
            return (Console.WindowHeight + Console.WindowWidth) == 0;
        }

        private void HandlePluginsLoaded(object sender, EventArgs e)
        {
            Task.Factory.StartNew(TerminalThreadFunc);
        }

        private void AddCommand(string name, CommandInfo info)
        {
            if (!commands.ContainsKey(name.ToLower()))
                commands.Add(name.ToLower(), info);
            else
                throw new ArgumentException("Command is already registered", name);
        }

        private void ShowHelp(string commandLine)
        {
            string command = null;
            if (commandLine.Contains(' '))
            {
                command = commandLine.Substring(commandLine.IndexOf(' ') + 1);
                if (command.Trim(' ').Length == 0)
                    command = null;
            }

            if (command == null)
            {
                WriteLine("List of available commands:");
                WriteLine();

                List<string> commandNames = new List<string>(commands.Keys);
                commandNames.Sort();

                while (commandNames.Count > 0)
                {
                    string commandName = commandNames[0];
                    CommandInfo info = commands[commandName];

                    commandNames.Remove(info.Name.ToLower());
                    info.Aliases.ForEach(name => commandNames.Remove(name.ToLower()));

                    WriteLine("  " + info.Name);
                    WriteLine("    " + info.HelpText);

                    if (info.Aliases.Count > 0)
                        WriteLine("    aliases: " + String.Join(", ", info.Aliases));
                }

                WriteLine();
            }
            else
            {
                CommandInfo info;
                if (IsValidCommand(command, out info))
                    WriteLine(info.HelpText);
                else
                    WriteLine("There is no such command: " + command);
            }
        }

        private void TerminalThreadFunc()
        {
            WriteLine("The server is up and running. Use 'quit' command to stop it...");

            UpdateCommandLineUnlocked();

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (cursorPosition > 0)
                    {
                        currentCommand.Remove(cursorPosition - 1, 1);
                        cursorPosition--;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Delete)
                {
                    if (cursorPosition < currentCommand.Length)
                        currentCommand.Remove(cursorPosition, 1);
                }
                else if (keyInfo.Key == ConsoleKey.LeftArrow)
                {
                    if (cursorPosition > 0)
                        cursorPosition--;
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow)
                {
                    if (cursorPosition < currentCommand.Length)
                        cursorPosition++;
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    string command = currentCommand.ToString();

                    currentCommand.Clear();
                    cursorPosition = 0;

                    WriteLine(commandLinePrefix + command);

                    CommandInfo info;
                    if (IsValidCommand(command, out info))
                        // Using new thread to avoid handlers crashing terminal plugin with unhandled exceptions.
                        Task.Factory.StartNew(() => info.Handler(command));
                    else if (command != "")
                        WriteLine("Invalid command");

                    lastCommand = command;
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    if (lastCommand != null)
                    {
                        currentCommand.Clear();
                        currentCommand.Append(lastCommand);
                    }
                }
                else if (IsText(keyInfo))
                {
                    if (cursorPosition == currentCommand.Length)
                        currentCommand.Append(keyInfo.KeyChar);
                    else
                        currentCommand.Insert(cursorPosition, keyInfo.KeyChar);
                    cursorPosition++;
                }

                lock (consoleLock)
                    UpdateCommandLineUnlocked();
            }
        }

        private bool IsValidCommand(string command, out CommandInfo info)
        {
            string key = command.ToLower();
            if (key.Contains(' '))
                key = key.Substring(0, key.IndexOf(' '));

            if (!commands.ContainsKey(key))
            {
                info = null;
                return false;
            }

            info = commands[key];
            if (commands[key].CaseSensitive)
                return info.Name.Equals(key) || info.Aliases.Contains(key);
            else
                return true;
        }

        /// <summary>
        /// Updates the command line. Make should you obtain consoleLock before calling this method.
        /// </summary>
        private void UpdateCommandLineUnlocked()
        {
            StringBuilder commandLine = new StringBuilder();
            commandLine.Append(commandLinePrefix);
            commandLine.Append(currentCommand.ToString());
            while (commandLine.Length < previousCommandLineLength)
                commandLine.Append(' ');

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(commandLine.ToString());
            Console.SetCursorPosition(3 + cursorPosition, Console.CursorTop);

            previousCommandLineLength = commandLine.Length;
        }

        private bool IsText(ConsoleKeyInfo info)
        {
            ConsoleKey consoleKey = info.Key;
            char consoleChar = info.KeyChar;

            return (consoleKey >= ConsoleKey.A && consoleKey <= ConsoleKey.Z) ||
                (consoleKey >= ConsoleKey.D0 && consoleKey <= ConsoleKey.D9) ||
                consoleChar == '?' || consoleChar == '"' || consoleChar == '+' || consoleChar == '(' ||
                consoleChar == ')' || consoleChar == '-' || consoleKey == ConsoleKey.Oem1 ||
                consoleKey == ConsoleKey.Oem7 || consoleKey == ConsoleKey.OemPeriod ||
                consoleKey == ConsoleKey.OemComma || consoleKey == ConsoleKey.OemMinus ||
                consoleKey == ConsoleKey.Add || consoleKey == ConsoleKey.Divide || consoleKey == ConsoleKey.Multiply ||
                consoleKey == ConsoleKey.Subtract || consoleKey == ConsoleKey.Oem102 ||
                consoleKey == ConsoleKey.Decimal || consoleKey == ConsoleKey.Spacebar;
        }

        private class CommandInfo
        {
            public string Name;
            public string HelpText;
            public bool CaseSensitive;
            public Action<string> Handler;
            public List<string> Aliases;
        }

        private object consoleLock = new object();

        private Dictionary<string, CommandInfo> commands = new Dictionary<string, CommandInfo>();

        string lastCommand = null;
        private const string commandLinePrefix = ">> ";
        private int previousCommandLineLength = commandLinePrefix.Length;
        private StringBuilder currentCommand = new StringBuilder();
        private int cursorPosition = 0;

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}
