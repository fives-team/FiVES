using FIVES;
using ServerSyncPlugin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using TerminalPlugin;

namespace ConfigScalabilityPlugin
{
    class ConfigScalabilityImpl : IConfigScalability
    {
        public ConfigScalabilityImpl()
        {
            LoadConfig();

            PluginManager.Instance.AddPluginLoadedHandler("Terminal", RegisterTerminalCommands);
        }

        private void LoadConfig()
        {
            string scalabilityConfigPath = this.GetType().Assembly.Location;
            Configuration config = ConfigurationManager.OpenExeConfiguration(scalabilityConfigPath);

            doi = new ConfigDoI();
            ReadDoIBoundaries(config, doi);
            ReadDoIComponents(config, doi);

            dor = new ConfigDoR();
            ReadDoRBoundaries(config, dor);

            ServerSync.LocalServer.DoI = doi;
            ServerSync.LocalServer.DoR = dor;
        }

        private void ReadDoRBoundaries(Configuration config, ConfigDoR dor)
        {
            dor.MinX = TryParseDouble(config, "DoR-MinX", Double.MinValue);
            dor.MaxX = TryParseDouble(config, "DoR-MaxX", Double.MaxValue);
            dor.MinY = TryParseDouble(config, "DoR-MinY", Double.MinValue);
            dor.MaxY = TryParseDouble(config, "DoR-MaxY", Double.MaxValue);
        }

        private static void ReadDoIComponents(Configuration config, ConfigDoI doi)
        {
            var doiComponents = config.AppSettings.Settings["DoI-Components"];
            if (doiComponents != null)
            {
                List<string> parsedDoIComponents = new List<string>(doiComponents.Value.Split(','));
                parsedDoIComponents = parsedDoIComponents.FindAll(c => c.Length > 0);

                // A single asterisk means all components are fine.
                if (parsedDoIComponents.Count == 1 && parsedDoIComponents[0] == "*")
                    return;

                doi.RelevantComponents = parsedDoIComponents;
            }
        }

        private void ReadDoIBoundaries(Configuration config, ConfigDoI doi)
        {
            doi.MinX = TryParseDouble(config, "DoI-MinX", Double.MinValue);
            doi.MaxX = TryParseDouble(config, "DoI-MaxX", Double.MaxValue);
            doi.MinY = TryParseDouble(config, "DoI-MinY", Double.MinValue);
            doi.MaxY = TryParseDouble(config, "DoI-MaxY", Double.MaxValue);
        }

        private double TryParseDouble(Configuration config, string property, double defaultValue)
        {
            if (config.AppSettings.Settings[property] == null)
                return defaultValue;

            double parsedValue;
            var value = config.AppSettings.Settings[property].Value;
            if (!Double.TryParse(value, out parsedValue))
                parsedValue = defaultValue;
            return parsedValue;
        }

        private void RegisterTerminalCommands()
        {
            Terminal.Instance.RegisterCommand("print-doi", "Prints current domain-of-interest", false, PrintDoI);
            Terminal.Instance.RegisterCommand("print-dor", "Prints current domain-of-responsibility", false, PrintDoR);

            Terminal.Instance.RegisterCommand("set-doi-minx", "Changes minimal X boundary for domain-of-interest", 
                false, ChangeDoIMinX);
            Terminal.Instance.RegisterCommand("set-doi-maxx", "Changes maximum X boundary for domain-of-interest", 
                false, ChangeDoIMaxX);
            Terminal.Instance.RegisterCommand("set-doi-miny", "Changes minimal Y boundary for domain-of-interest", 
                false, ChangeDoIMinY);
            Terminal.Instance.RegisterCommand("set-doi-maxy", "Changes maximum Y boundary for domain-of-interest", 
                false, ChangeDoIMaxY);
            Terminal.Instance.RegisterCommand("set-doi-components", 
                "Changes relevant components for domain-of-interest", false, ChangeDoIComponents);

            Terminal.Instance.RegisterCommand("set-dor-minx", 
                "Changes minimal X boundary for domain-of-responsibility", false, ChangeDoRMinX);
            Terminal.Instance.RegisterCommand("set-dor-maxx", 
                "Changes maximum X boundary for domain-of-responsibility", false, ChangeDoRMaxX);
            Terminal.Instance.RegisterCommand("set-dor-miny", 
                "Changes minimal Y boundary for domain-of-responsibility", false, ChangeDoRMinY);
            Terminal.Instance.RegisterCommand("set-dor-maxy", 
                "Changes maximum Y boundary for domain-of-responsibility", false, ChangeDoRMaxY);
        }

        private void PrintDoI(string commandLine)
        {
            Terminal.Instance.WriteLine("Current domain-of-interest: " + doi);
        }

        private void PrintDoR(string commandLine)
        {
            Terminal.Instance.WriteLine("Current domain-of-responsibility: " + dor);
        }

        private void ChangeDoIMinX(string commandLine)
        {
            double value;
            if (Double.TryParse(commandLine.Substring(commandLine.IndexOf(' ') + 1), out value))
            {
                doi.MinX = value;
                ServerSync.LocalServer.DoI = doi;
            }
        }

        private void ChangeDoIMaxX(string commandLine)
        {
            double value;
            if (Double.TryParse(commandLine.Substring(commandLine.IndexOf(' ') + 1), out value))
            {
                doi.MaxX = value;
                ServerSync.LocalServer.DoI = doi;
            }
        }

        private void ChangeDoIMinY(string commandLine)
        {
            double value;
            if (Double.TryParse(commandLine.Substring(commandLine.IndexOf(' ') + 1), out value))
            {
                doi.MinY = value;
                ServerSync.LocalServer.DoI = doi;
            }
        }

        private void ChangeDoIMaxY(string commandLine)
        {
            double value;
            if (Double.TryParse(commandLine.Substring(commandLine.IndexOf(' ') + 1), out value))
            {
                doi.MaxY = value;
                ServerSync.LocalServer.DoI = doi;
            }
        }

        private void ChangeDoIComponents(string commandLine)
        {
            int spaceIndex = commandLine.IndexOf(' ');
            if (spaceIndex == -1)
            {
                doi.RelevantComponents = null;
                ServerSync.LocalServer.DoI = doi;
            }
            else
            {
                string componentsArgument = commandLine.Substring(spaceIndex + 1);
                List<string> parsedDoIComponents = new List<string>(componentsArgument.Split(','));
                if (parsedDoIComponents.Find(c => c.Length > 0) != null)
                {
                    doi.RelevantComponents = parsedDoIComponents;
                    ServerSync.LocalServer.DoI = doi;
                }
            }
        }

        private void ChangeDoRMinX(string commandLine)
        {
            double value;
            if (Double.TryParse(commandLine.Substring(commandLine.IndexOf(' ') + 1), out value))
            {
                dor.MinX = value;
                ServerSync.LocalServer.DoR = dor;
            }
        }

        private void ChangeDoRMaxX(string commandLine)
        {
            double value;
            if (Double.TryParse(commandLine.Substring(commandLine.IndexOf(' ') + 1), out value))
            {
                dor.MaxX = value;
                ServerSync.LocalServer.DoR = dor;
            }
        }

        private void ChangeDoRMinY(string commandLine)
        {
            double value;
            if (Double.TryParse(commandLine.Substring(commandLine.IndexOf(' ') + 1), out value))
            {
                dor.MinY = value;
                ServerSync.LocalServer.DoR = dor;
            }
        }

        private void ChangeDoRMaxY(string commandLine)
        {
            double value;
            if (Double.TryParse(commandLine.Substring(commandLine.IndexOf(' ') + 1), out value))
            {
                dor.MaxY = value;
                ServerSync.LocalServer.DoR = dor;
            }
        }

        ConfigDoI doi;
        ConfigDoR dor;
    }
}
