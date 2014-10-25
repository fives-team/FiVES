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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using TestingPlugin;

namespace WebTests
{
    class FIVESServerInstance
    {
        public FIVESServerInstance()
        {
            // Create new directory for the test.
            testDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..", "FIVES-Test-" + (uniqueId++));

            // Copy the binaries.
            DirectoryCopy(Directory.GetCurrentDirectory(), testDirectory, true);
        }

        public void Start()
        {
            if (plugins != null && protocols != null)
                throw new Exception("Missing protocol and plugin configuration");

            if (testingClientURI != null && testingServerURI != null)
                throw new Exception("Missing testing service configuration");

            CreateServerConfig();
            StartTestingService();

            // Start the server process.
            ProcessStartInfo serverInfo = new ProcessStartInfo(Path.Combine(testDirectory, "FIVES.exe"));
            serverInfo.WorkingDirectory = testDirectory;
            serverInfo.UseShellExecute = false;
            serverInfo.WindowStyle = ProcessWindowStyle.Hidden;
            serverInfo.RedirectStandardInput = true;
            serverInfo.RedirectStandardOutput = true;
            serverInfo.RedirectStandardError = true;

            // Wait for the server process to report when server is ready.
            AutoResetEvent serverHasStarted = new AutoResetEvent(false);
            EventHandler handler = (sender, args) => serverHasStarted.Set();
            testingClient.ServerReady += handler;
            process = Process.Start(serverInfo);
            serverHasStarted.WaitOne();
            testingClient.ServerReady -= handler;
        }

        private void CreateServerConfig()
        {
            string serverConfigPath = Path.Combine(testDirectory, "FIVES.exe.config");
            var serverConfig = @"<?xml version=""1.0"" encoding=""utf-8""?>
                <configuration>
                    <configSections>
                        <section name=""nlog"" type=""NLog.Config.ConfigSectionHandler, NLog""/>
                    </configSections>

                    <appSettings>
                        <add key=""PluginDir"" value=""."" />
                        <add key=""PluginWhiteList"" value=""" + String.Join(",", plugins) + @""" />
                        <add key=""ProtocolDir"" value=""."" />
                        <add key=""ProtocolWhiteList"" value=""" + String.Join(",", protocols) + @"""/>
                    </appSettings>

                    <Testing>
                        <add key=""testingClient"" value=""" + testingClientURI + @"""/>
                        <add key=""testingServer"" value=""" + testingServerURI + @"""/>
                    </Testing>

                    <nlog xmlns=""http://www.nlog-project.org/schemas/NLog.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
                        <targets>
                            <target name=""logfile"" xsi:type=""File"" fileName=""FIVES.log"" layout=""${longdate}|${level:uppercase=true}|${callsite}|${logger}|${message}|${exception:format=tostring}"" />
                        </targets>

                        <rules>
                            <logger name=""*"" minlevel=""Debug"" writeTo=""logfile"" />
                        </rules>
                    </nlog>
                </configuration>";
            File.WriteAllText(serverConfigPath, serverConfig);
        }

        public void Stop()
        {
            // Terminate the server process.
            testingClient.ServerChannel.ShutdownServer();
            process.WaitForExit();

            // Delete testing directory.
            Directory.Delete(testDirectory, true);

            StopTestingService();
        }

        public void ConfigureClientManagerPorts(int listeningPort)
        {
            string clientManagerConfigPath = Path.Combine(testDirectory, "clientManagerServer.json");
            string clientManagerConfig = @"
                {
                  ""info"": ""ClientManagerServer"",
                  ""idlURL"": ""../../WebClient/kiara/fives.kiara"",
                  ""servers"": [{
                    ""services"": ""*"",
                    ""protocol"": {
                      ""name"": ""websocket-json"",
                      ""port"": " + listeningPort + @",
                    }
                  }]
                }
            ";
            File.WriteAllText(clientManagerConfigPath, clientManagerConfig);
        }

        public void ConfigurePluginsAndProtocols(string[] plugins, string[] protocols)
        {
            this.plugins = plugins;
            this.protocols = protocols;

            foreach (string plugin in plugins)
            {
                string pluginPath = Path.Combine(testDirectory, plugin + ".dll");
                if (!File.Exists(pluginPath))
                    throw new Exception("Plugin assembly " + pluginPath + " is not found.");
            }
        }

        public void ConfigureTestingService(int clientPort, int serverPort)
        {
            this.testingClientURI = "net.tcp://localhost:" + clientPort + "/FIVESTestingService";
            this.testingServerURI = "net.tcp://localhost:" + serverPort + "/FIVESTestingService";
        }

        public void ConfigureServerSyncPorts(int listeningPort, int[] remotePorts)
        {
            string serverConfigPath = Path.Combine(testDirectory, "serverSyncServer.json");
            string serverConfig = @"
                {
                  ""info"": ""ServerSyncServer"",
                  ""idlURL"": ""serverSync.kiara"",
                  ""servers"": [{
                    ""services"": ""*"",
                    ""protocol"": {
                      ""name"": ""websocket-json"",
                      ""port"": " + listeningPort + @",
                    }
                  }]
                }
            ";
            File.WriteAllText(serverConfigPath, serverConfig);

            string clientConfigPath = Path.Combine(testDirectory, "serverSyncClient.json");
            StringBuilder clientConfig = new StringBuilder();
            clientConfig.Append("{'info':'ScalabilitySyncClient','idlURL':'syncServer.kiara','servers':[");
            foreach (int remotePort in remotePorts)
            {
                clientConfig.Append("{'services':'*','protocol':{'name':'websocket-json','port':" + remotePort +
                                    ",'host':'127.0.0.1'}},");
            }
            clientConfig.Append("]}");
            File.WriteAllText(clientConfigPath, clientConfig.ToString());
        }

        private void StartTestingService()
        {
            testingClient = new TestingClient();

            serviceHost = new ServiceHost(testingClient);
            serviceHost.AddServiceEndpoint(typeof(ITestingClient), new NetTcpBinding(), testingClientURI);
            serviceHost.Open();
        }

        private void StopTestingService()
        {
            serviceHost.Close();
        }

        // Taken from http://msdn.microsoft.com/en-us/library/bb762914(v=vs.110).aspx
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory exists, delete it first.
            if (Directory.Exists(destDirName))
                Directory.Delete(destDirName, copySubDirs);
            Directory.CreateDirectory(destDirName);


            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        // The server process.
        Process process;

        // Global unique used to create testing directories, where FIVES is copied.
        static int uniqueId = 1;

        // Global singleton implementing testing service, which is used by the Testing plugin on the server.
        TestingClient testingClient = null;

        // Service host implementing the testing service.
        ServiceHost serviceHost;

        // Testing directory. Created when the object is constructed. All FIVES binaries are copied here.
        private string testDirectory;

        // Settings for the main configuration file.
        private string[] plugins = null;
        private string[] protocols = null;
        private string testingClientURI = null;
        private string testingServerURI = null;
    }
}
