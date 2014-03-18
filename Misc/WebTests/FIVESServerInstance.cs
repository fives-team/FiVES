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
            TestDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..", "FIVES-Test-" + (uniqueId++));

            // Copy the binaries.
            DirectoryCopy(Directory.GetCurrentDirectory(), TestDirectory, true);
        }

        public void Start()
        {
            if (numInstances++ == 0)
                StartTestingService();

            // Start the server process.
            ProcessStartInfo serverInfo = new ProcessStartInfo(Path.Combine(TestDirectory, "FIVES.exe"));
            serverInfo.WorkingDirectory = TestDirectory;
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
            Directory.Delete(TestDirectory, true);

            if (--numInstances == 0)
                StopTestingService();
        }

        // Testing directory. Created when the object is constructed. All FIVES binaries are copied here.
        public string TestDirectory { get; private set; }

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
    }
}
