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
using System.Diagnostics;
using System.Text;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.IO;

namespace WebTests
{
    [TestFixture]
    public class LoginTests
    {
        private IWebDriver driver;
        private FIVESServerInstance server;
        private WebServer webServer;

        private const int fivesServerPort = 34837;
        private const int webServerPort = 34838;

        [TestFixtureSetUp]
        public void StartServer()
        {
            webServer = new WebServer();
            webServer.ServerPort = webServerPort;
            webServer.RootDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "WebClient");
            webServer.Start();

            server = new FIVESServerInstance();
            server.ConfigureClientManagerPorts(fivesServerPort);
            server.ConfigurePluginsAndProtocols(
                new string[] { "Auth", "Avatar", "ClientManager", "KIARA", "Location", "Motion", "Testing",
                               "Renderable", "EventLoop", "Editing", "KeyframeAnimation" },
                new string[] { "WebSocketJSON" });
            server.Start();
        }

        [TestFixtureTearDown]
        public void StopServer()
        {
            server.Stop();
            webServer.Stop();
        }

        [SetUp]
        public void StartChrome()
        {
            driver = Tools.CreateDriver();
            driver.Navigate().GoToUrl(String.Format(
                "http://localhost:{0}/client.xhtml#FIVESTesting&OverrideServerPort={1}", webServerPort,
                fivesServerPort));
        }

        [TearDown]
        public void QuitChrome()
        {
            driver.Quit();
        }

        [Test]
        public void ShouldLoginWithTest123()
        {
            Tools.Login(driver, "test", "123");
        }

        [Test]
        public void ShouldLoginWithTestWithoutPassword()
        {
            Tools.Login(driver, "test", "");
        }

        [Test]
        public void ShouldLoginWithWithRandomUserName()
        {
            Tools.Login(driver, "random" + new Random().Next(), "");
        }
    }
}
