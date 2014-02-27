﻿using System;
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
            TestDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..", "FIVES-Test-" + (testCounter++));

            // Copy the binaries.
            DirectoryCopy(Directory.GetCurrentDirectory(), TestDirectory, true);
        }

        public void Start()
        {
            // Create a service host.
            var serviceHost = new ServiceHost(testingService);
            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            serviceHost.AddServiceEndpoint(typeof(ITestingService), binding, Testing.ServiceURI);
            serviceHost.Open();

            // Start the server process.
            ProcessStartInfo serverInfo = new ProcessStartInfo(Path.Combine(TestDirectory, "FIVES.exe"));
            serverInfo.WorkingDirectory = TestDirectory;
            serverInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process = Process.Start(serverInfo);

            // Wait for the server process to report when server is ready.
            AutoResetEvent serverHasStarted = new AutoResetEvent(false);
            EventHandler handler = (sender, args) => serverHasStarted.Set();
            testingService.ServerReady += handler;
            serverHasStarted.WaitOne();
            testingService.ServerReady -= handler;

            // Close service host.
            serviceHost.Close();
        }

        public void Stop()
        {
            // Terminate the server process.
            process.Kill();
            process.WaitForExit();

            // Delete testing directory.
            Directory.Delete(TestDirectory, true);
        }

        // Testing directory. Created when the object is constructed. All FIVES binaries are copied here.
        public string TestDirectory { get; private set; }

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

        // Counter to generate different names for testing folders.
        static int testCounter = 1;

        // Global singleton implementing testing service, which is used by the Testing plugin on the server.
        static TestingService testingService = new TestingService();
    }
}
