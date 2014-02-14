using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace WebTests
{
    [TestFixture]
    public class AvatarTests
    {
        private Process server;

        [TestFixtureSetUp]
        public void StartServer()
        {
            ProcessStartInfo serverInfo = new ProcessStartInfo("FIVES.exe");
            serverInfo.WindowStyle = ProcessWindowStyle.Hidden;
            server = Process.Start(serverInfo);
        }

        [TestFixtureTearDown]
        public void StopServer()
        {
            server.Kill();
        }

        [Test]
        public void ShouldMoveAvatar()
        {
            IWebDriver driver = new ChromeDriver();
            try
            {
                driver.Navigate().GoToUrl("http://localhost/projects/test-client/client.xhtml");
                Tools.Login(driver, "1", "");

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
                IJavaScriptExecutor jsExecutor = driver as IJavaScriptExecutor;

                string avatarGuid = (string)wait.Until(d => jsExecutor.ExecuteScript("return FIVES.AvatarEntityGuid"));
                IWebElement avatarTransform = driver.FindElement(By.Id("transform-" + avatarGuid));

                string translationBefore = avatarTransform.GetAttribute("translation");

                IHasInputDevices input = driver as IHasInputDevices;
                input.Keyboard.PressKey("w");
                Thread.Sleep(500);
                input.Keyboard.ReleaseKey("w");

                string translationAfter = avatarTransform.GetAttribute("translation");

                Assert.AreNotEqual(translationBefore, translationAfter);
            }
            finally
            {
                driver.Quit();
            }
        }

        [Test]
        public void ShouldSynchronizeAvatarMovement()
        {
            IWebDriver driver1 = new ChromeDriver();
            IWebDriver driver2 = new ChromeDriver();

            try
            {
                driver1.Navigate().GoToUrl("http://localhost/projects/test-client/client.xhtml");
                driver2.Navigate().GoToUrl("http://localhost/projects/test-client/client.xhtml");

                Tools.Login(driver1, "1", "");
                Tools.Login(driver2, "2", "");

                // TODO: Wait until both browsers have loaded the scene

                WebDriverWait wait1 = new WebDriverWait(driver1, TimeSpan.FromSeconds(10));
                IJavaScriptExecutor jsExecutor1 = driver1 as IJavaScriptExecutor;
                string avatarGuid = (string)wait1.Until(
                    d => jsExecutor1.ExecuteScript("return FIVES.AvatarEntityGuid"));

                IWebElement avatarTransform1 = driver1.FindElement(By.Id("transform-" + avatarGuid));
                IWebElement avatarTransform2 = driver2.FindElement(By.Id("transform-" + avatarGuid));

                string translation1Before = avatarTransform1.GetAttribute("translation");
                string translation2Before = avatarTransform2.GetAttribute("translation");

                IHasInputDevices input1 = driver1 as IHasInputDevices;
                input1.Keyboard.PressKey("w");
                Thread.Sleep(500);
                input1.Keyboard.ReleaseKey("w");

                // TODO: wait until avatar stops moving in both driver1 and driver2

                string translation1After = avatarTransform1.GetAttribute("translation");
                string translation2After = avatarTransform2.GetAttribute("translation");

                Assert.AreEqual(translation1Before, translation2Before);
                Assert.AreEqual(translation1After, translation2After);
            }
            finally
            {
                driver1.Quit();
                driver2.Quit();
            }
        }
    }
}
