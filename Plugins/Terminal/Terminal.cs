using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalPlugin
{
    public class Terminal
    {
        public static Terminal Instance;

        public Terminal(ApplicationController controller)
        {
            this.controller = controller;

            RegisterCommand("help", "Shows help text for all commands", false, ShowHelp);
            RegisterCommand("quit", "Shuts the server down.", false, ShutDown, new List<string> { "q" });
            RegisterCommand("clean", "Removes all entities from the server", false, RemoveAllEntities);

            Task.Factory.StartNew(TerminalThreadFunc);
        }

        public void WriteLine()
        {
            WriteLine("");
        }

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
        /// <param name="commands">List of command aliases.</param>
        /// <param name="helpText">Help text used for this command in the help command.</param>
        /// <param name="caseSensitive">True if name is case-sensitive.</param>
        /// <param name="aliases">An optional set of aliases.</param>
        /// <param name="handler">Handler to be invoked when command is executed.</param>
        public void RegisterCommand(string command, string helpText, bool caseSensitive, Action<string> handler,
                                    List<string> aliases = null)
        {
            if (command == null)
                throw new ArgumentException("Command can not be null", "command");

            if (handler == null)
                throw new ArgumentException("Handler can not be null", "handler");

            var info = new CommandInfo
            {
                Command = command,
                HelpText = helpText,
                CaseSensitive = caseSensitive,
                Handler = handler,
                Aliases = aliases
            };

            lock (commands)
            {
                AddCommand(command, info);

                if (aliases != null)
                {
                    foreach (string alias in aliases)
                        AddCommand(alias, info);
                }
            }
        }

        private void AddCommand(string alias, CommandInfo info)
        {
            if (!commands.ContainsKey(alias.ToLower()))
                commands.Add(alias.ToLower(), info);
            else
                throw new ArgumentException("Command is already registered", alias);
        }

        private void ShutDown(string commandLine)
        {
            WriteLine("Shutting down the server...");
            controller.Terminate();
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

                    if (info.Aliases.Count > 0)
                        commandNames.RemoveAll(name => info.Aliases.Contains(name));

                    PrintCommandHelp(info);
                }
            }
            else
            {
                CommandInfo info;
                if (IsValidCommand(command, out info))
                    PrintCommandHelp(info);
                else
                    WriteLine("There is no such command: " + command);
            }
        }

        private void PrintCommandHelp(CommandInfo info)
        {
            WriteLine("  " + info.Command);
            WriteLine("    " + info.HelpText);

            if (info.Aliases.Count > 0)
                WriteLine("    aliases: " + String.Join(", ", info.Aliases));
        }

        private void RemoveAllEntities(string commandLine)
        {
            World.Instance.Clear();
            WriteLine("Removed all entities");
        }

        private void TerminalThreadFunc()
        {
            Console.WriteLine("The server is up and running. Press 'q' to stop it...");
            while (Console.ReadKey().KeyChar != 'q') ;

            // Example code for controlling the cursor. Works on Windows. Need to be tested on Linux.

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
                    WriteLine(">> " + command);

                    CommandInfo info;
                    if (IsValidCommand(command, out info))
                        info.Handler(command);
                    else
                        WriteLine("Invalid command");

                    currentCommand.Clear();
                    cursorPosition = 0;
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
            {
                if (info.Command.Equals(key))
                    return true;
                else if (info.Aliases != null && info.Aliases.Contains(key))
                    return true;
                else
                    return false;
            }

            return true;
        }

        private void UpdateCommandLineUnlocked()
        {
            StringBuilder commandLine = new StringBuilder();
            commandLine.Append(">> ");
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
                consoleKey == ConsoleKey.Decimal;
        }

        private class CommandInfo
        {
            public string Command;
            public string HelpText;
            public bool CaseSensitive;
            public Action<string> Handler;
            public List<string> Aliases;
        }

        private object consoleLock = new object();

        private ApplicationController controller;

        private Dictionary<string, CommandInfo> commands = new Dictionary<string, CommandInfo>();

        private int previousCommandLineLength = 3;  // empty command line is ">> "
        private StringBuilder currentCommand = new StringBuilder();
        private int cursorPosition = 0;
    }
}
