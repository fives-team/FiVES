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
using System.Diagnostics;
using System.Text;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace WebTests
{
    [TestFixture]
    public class LoginTests
    {
        private IWebDriver driver;
        private FIVESServerInstance server;

        [TestFixtureSetUp]
        public void StartServer()
        {
            server = new FIVESServerInstance();
            server.ConfigureClientManagerPorts(34837);
            server.ConfigurePluginsAndProtocols(
                new string[] { "Auth", "Avatar", "ClientManager", "SINFONI", "Location", "Motion", "Testing",
                               "Renderable", "EventLoop", "Editing", "KeyframeAnimation" },
                new string[] { "WebSocketJSON" });
            server.Start();
        }

        [TestFixtureTearDown]
        public void StopServer()
        {
            server.Stop();
        }

        [SetUp]
        public void StartChrome()
        {
            driver = Tools.CreateDriver();
            driver.Navigate().GoToUrl(
                "http://localhost/projects/test-client/client.xhtml#FIVESTesting&OverrideServerPort=34837");
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
