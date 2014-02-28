using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Text;

namespace WebTests
{
    [TestFixture]
    public class ServerSyncTests
    {
        private FIVESServerInstance server1;
        private FIVESServerInstance server2;

        [TestFixtureSetUp]
        public void StartServer()
        {
            server1 = new FIVESServerInstance();
            ConfigureServerSyncPorts(server1, 43745, new int[] {});
            ConfigureClientManagerPorts(server1, 34837);
            server1.Start();

            server2 = new FIVESServerInstance();
            ConfigureServerSyncPorts(server2, 43746, new int[] { 43745 });
            ConfigureClientManagerPorts(server2, 34839);
            server2.Start();
        }

        [TestFixtureTearDown]
        public void StopServer()
        {
            server1.Stop();
            server2.Stop();
        }

        [Test]
        public void ShouldSynchronizeAvatarMovementAcrossServers()
        {
            IWebDriver driver1 = Tools.CreateDriver();
            IWebDriver driver2 = Tools.CreateDriver();

            try
            {
                driver1.Navigate().GoToUrl(
                    "http://localhost/projects/test-client/client.xhtml#FIVESTesting&OverrideServerPort=34837");
                driver2.Navigate().GoToUrl(
                    "http://localhost/projects/test-client/client.xhtml#FIVESTesting&OverrideServerPort=34839");

                Tools.Login(driver1, "1", "");
                Tools.Login(driver2, "2", "");

                WebDriverWait wait = new WebDriverWait(driver1, TimeSpan.FromSeconds(20));
                IJavaScriptExecutor jsExecutor1 = driver1 as IJavaScriptExecutor;

                // Wait for the FIVES.AvatarEntityGuid to become available.
                string avatarGuid = (string)wait.Until(
                    d => jsExecutor1.ExecuteScript("return FIVES.AvatarEntityGuid"));

                // Wait until transform element for the same avatar becomes available in both browsers.
                IWebElement avatarTransform1 = wait.Until(d => driver1.FindElement(By.Id("transform-" + avatarGuid)));
                IWebElement avatarTransform2 = wait.Until(d => driver2.FindElement(By.Id("transform-" + avatarGuid)));

                string startTranslation = avatarTransform1.GetAttribute("translation");

                jsExecutor1.ExecuteScript("$(document).trigger({type: 'keydown', which: 87, keyCode: 87})");

                // Wait until avatar starts to move.
                wait.Until(d => avatarTransform1.GetAttribute("translation") != startTranslation);

                jsExecutor1.ExecuteScript("$(document).trigger({type: 'keyup', which: 87, keyCode: 87})");

                // Wait until avatars are synchronized.
                wait.Until(d => avatarTransform1.GetAttribute("translation") ==
                    avatarTransform2.GetAttribute("translation"));
            }
            finally
            {
                driver1.Quit();
                driver2.Quit();
            }
        }

        private void ConfigureServerSyncPorts(FIVESServerInstance server, int listeningPort, int[] remotePorts)
        {
            string serverConfigPath = Path.Combine(server.TestDirectory, "serverSyncServer.json");
            string serverConfig = File.ReadAllText(serverConfigPath);
            serverConfig = serverConfig.Replace("43745", listeningPort.ToString());
            File.WriteAllText(serverConfigPath, serverConfig);

            string clientConfigPath = Path.Combine(server.TestDirectory, "serverSyncClient.json");
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

        private void ConfigureClientManagerPorts(FIVESServerInstance server, int listeningPort)
        {
            string clientManagerConfigPath = Path.Combine(server.TestDirectory, "clientManagerServer.json");
            string clientManagerConfig = File.ReadAllText(clientManagerConfigPath);
            clientManagerConfig = clientManagerConfig.Replace("34837", listeningPort.ToString());
            File.WriteAllText(clientManagerConfigPath, clientManagerConfig);
        }
    }
}
