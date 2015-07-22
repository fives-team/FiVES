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
            if (numInstances++ == 0)
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
            testingService.ServerReady += handler;
            process = Process.Start(serverInfo);
            serverHasStarted.WaitOne();
            testingService.ServerReady -= handler;
        }

        public void Stop()
        {
            // Terminate the server process.
            process.Kill();
            process.WaitForExit();

            // Delete testing directory.
            Directory.Delete(testDirectory, true);

            if (--numInstances == 0)
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

            foreach (string plugin in plugins)
            {
                string pluginPath = Path.Combine(testDirectory, plugin + ".dll");
                if (!File.Exists(pluginPath))
                    throw new Exception("Plugin assembly " + pluginPath + " is not found.");
            }
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
            testingService = new TestingService();

            serviceHost = new ServiceHost(testingService);
            serviceHost.AddServiceEndpoint(typeof(ITestingService), new NetTcpBinding(), Testing.ServiceURI);
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

        // Number of server instances. This is used to initialize the testing service for the first instance is started
        // and to close it when the last instance is stopped.
        static int numInstances = 0;

        // Global singleton implementing testing service, which is used by the Testing plugin on the server.
        static TestingService testingService;

        // Service host implementing the testing service.
        static ServiceHost serviceHost;

        // Testing directory. Created when the object is constructed. All FIVES binaries are copied here.
        private string testDirectory;
    }
}
