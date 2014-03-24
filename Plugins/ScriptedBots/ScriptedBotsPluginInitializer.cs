using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;
using System.IO;

namespace ScriptedBotsPlugin
{
    public class ScriptedBotsPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get
            {
                return "ScriptedBots";
            }
        }

        public List<string> PluginDependencies
        {
            get
            {
                return new List<string> { "Scripting" };
            }
        }

        public List<string> ComponentDependencies
        {
            get
            {
                return new List<string> { "location", "mesh", "scripting" };
            }
        }

        public void Initialize()
        {
            LoadConfig();
            LoadScript();

            for (int i = 0; i < numBotsToCreate; i++)
                CreateScriptedBot();
        }

        private void LoadScript()
        {
            botScript = File.ReadAllText(GetResourceFile("botScript.js"));
        }

        private void CreateScriptedBot()
        {
            var botEntity = new Entity();
            botEntity["mesh"]["uri"] = "";
            botEntity["scripting"]["serverScript"] = botScript;
            botEntity["scripting"]["serverScriptDeps"] = "eventLoop.addTickFiredHandler != undefined && " +
                "eventLoop.intervalMs != undefined && entity['location'] != null && entity['motion'] != null";
            botEntity["location"]["position"] =
                new Vector((float)(random.NextDouble() * 13 + 1), 0f, (float)(random.NextDouble() * 13 + 1));
            World.Instance.Add(botEntity);
        }

        /// <summary>
        /// Converts a file name to the URI that point to the file as if it was located in the same directory as the
        /// current assembly.
        /// </summary>
        /// <param name="configFilename"></param>
        /// <returns></returns>
        private string GetResourceFile(string configFilename)
        {
            return Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), configFilename);
        }

        private void LoadConfig()
        {
            // TODO: replace with configuration file when a per-plugin configuration system is implemented
            numBotsToCreate = 1;
        }

        public void Shutdown()
        {
        }

        int numBotsToCreate;
        string botScript;
        Random random = new Random();
    }
}
