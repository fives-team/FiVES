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
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace WebTests
{
    [TestFixture]
    public class AvatarTests
    {
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

        [Test]
        public void ShouldMoveAvatar()
        {
            IWebDriver driver = Tools.CreateDriver();
            try
            {
                driver.Navigate().GoToUrl(
                    "http://localhost/projects/test-client/client.xhtml#FIVESTesting&OverrideServerPort=34837");
                Tools.Login(driver, "1", "");

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                IJavaScriptExecutor jsExecutor = driver as IJavaScriptExecutor;

                // Wait for the FIVES.AvatarEntityGuid to become available.
                string avatarGuid = (string)wait.Until(d => jsExecutor.ExecuteScript("return FIVES.AvatarEntityGuid"));

                // Wait until avatar's transform element becomes available.
                IWebElement avatarTransform = wait.Until(d => d.FindElement(By.Id("transform-" + avatarGuid)));

                string startTranslation = avatarTransform.GetAttribute("translation");

                jsExecutor.ExecuteScript("$(document).trigger({type: 'keydown', which: 87, keyCode: 87})");

                // Wait until avatar starts to move.
                wait.Until(d => avatarTransform.GetAttribute("translation") != startTranslation);
            }
            finally
            {
                driver.Quit();
            }
        }

        [Test]
        public void ShouldSynchronizeAvatarMovement()
        {
            IWebDriver driver1 = Tools.CreateDriver();
            IWebDriver driver2 = Tools.CreateDriver();

            try
            {
                driver1.Navigate().GoToUrl(
                    "http://localhost/projects/test-client/client.xhtml#FIVESTesting&OverrideServerPort=34837");
                driver2.Navigate().GoToUrl(
                    "http://localhost/projects/test-client/client.xhtml#FIVESTesting&OverrideServerPort=34837");

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
