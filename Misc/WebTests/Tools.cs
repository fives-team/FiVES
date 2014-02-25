using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;
using TestingPlugin;
using System.ServiceModel;
using System.Threading;

namespace WebTests
{
    static class Tools
    {
        public static void Login(IWebDriver driver, string username, string password)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            IWebElement signinBtn = wait.Until<IWebElement>(d => d.FindElement(By.Id("signin-btn")));
            IWebElement signinModal = wait.Until<IWebElement>(d => d.FindElement(By.Id("signin-modal")));

            wait.Until<bool>(d => signinBtn.Displayed && signinBtn.Enabled);

            var jsExecutor = driver as IJavaScriptExecutor;
            jsExecutor.ExecuteScript(@"$('#signin-login').val(arguments[0]);
                $('#signin-password').val(arguments[1]);
                $('#signin-btn').click();", username, password);

            wait.Until<bool>(d => !signinModal.Displayed);
        }

        public static IWebDriver CreateDriver()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--disable-cache");
            var driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl("http://localhost/projects/test-client/client.xhtml");
            return driver;
        }

        public static void StartServer()
        {
            // Recreate service host (closed service host can't be opened again).
            serviceHost = new ServiceHost(testingService);
            NetTcpBinding binding = new NetTcpBinding();
            serviceHost.AddServiceEndpoint(typeof(ITestingService), binding, Testing.ServiceURI);
            serviceHost.Open();

            // Start the server process.
            ProcessStartInfo serverInfo = new ProcessStartInfo("FIVES.exe");
            serverInfo.WindowStyle = ProcessWindowStyle.Hidden;
            server = Process.Start(serverInfo);

            // Wait for the server process to report when server is ready.
            AutoResetEvent serverHasStarted = new AutoResetEvent(false);
            testingService.ServerReady += (sender, args) => serverHasStarted.Set();
            serverHasStarted.WaitOne();
        }

        public static void StopServer()
        {
            // Terminate the server process.
            server.Kill();

            // Close the service host to terminate its thread. Otherwise tests fail when there are remaining threads.
            serviceHost.Close();
        }

        // Global singleton implementing testing service, which is used by the Testing plugin on the server.
        static TestingService testingService = new TestingService();

        // Service host which hosts the testing service.
        static ServiceHost serviceHost;

        // The server process.
        static Process server;
    }
}
