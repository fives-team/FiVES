using NUnit.Framework;
using System.Text;
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;

namespace WebTests
{
    [TestFixture]
    public class LoginTests
    {
        private IWebDriver driver;
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

        [SetUp]
        public void StartChrome()
        {
            driver = new ChromeDriver();
            driver.Navigate().GoToUrl("http://localhost/projects/test-client/client.xhtml");
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
