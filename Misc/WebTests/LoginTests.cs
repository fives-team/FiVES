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
