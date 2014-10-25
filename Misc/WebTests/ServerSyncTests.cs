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
        private WebServer webServer;

        private const int fivesServerPort1 = 34837;
        private const int fivesServerPort2 = 34838;
        private const int webServerPort = 34839;
        private const int fivesTestingClientPort1 = 34840;
        private const int fivesTestingServerPort1 = 34841;
        private const int fivesTestingServerPort2 = 34842;
        private const int fivesTestingClientPort2 = 34843;

        [TestFixtureSetUp]
        public void StartServer()
        {
            webServer = new WebServer();
            webServer.ServerPort = webServerPort;
            webServer.RootDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "WebClient");
            webServer.Start();

            server1 = new FIVESServerInstance();
            server1.ConfigureServerSyncPorts(43745, new int[] {});
            server1.ConfigureClientManagerPorts(34837);
            server1.ConfigureTestingService(fivesTestingClientPort1, fivesTestingServerPort2);
            server1.ConfigurePluginsAndProtocols(new string[] { "Auth", "Avatar", "ClientManager", "KIARA", "Location",
                "Motion", "Testing", "Renderable", "EventLoop", "Editing", "ServerSync", "ConfigScalability",
                "Scalability", "KeyframeAnimation" }, new string[] { "WebSocketJSON" });
            server1.Start();

            server2 = new FIVESServerInstance();
            server2.ConfigureServerSyncPorts(43746, new int[] { 43745 });
            server2.ConfigureClientManagerPorts(34839);
            server2.ConfigureTestingService(fivesTestingClientPort1, fivesTestingServerPort2);
            server2.ConfigurePluginsAndProtocols(new string[] { "Auth", "Avatar", "ClientManager", "KIARA", "Location",
                "Testing", "Renderable", "EventLoop", "Editing", "ServerSync", "ConfigScalability",
                "Scalability", "KeyframeAnimation" }, new string[] { "WebSocketJSON" });
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
                driver1.Navigate().GoToUrl(String.Format(
                    "http://localhost:{0}/client.xhtml#FIVESTesting&OverrideServerPort={1}", webServerPort,
                    fivesServerPort1));
                driver2.Navigate().GoToUrl(String.Format(
                    "http://localhost:{0}/client.xhtml#FIVESTesting&OverrideServerPort={1}", webServerPort,
                    fivesServerPort2));

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
    }
}
