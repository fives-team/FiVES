using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebTests
{
    static class Tools
    {
        public static void Login(IWebDriver driver, string username, string password)
        {
            var jsExecutor = driver as IJavaScriptExecutor;
            var signinBtn = driver.FindElement(By.Id("signin-btn"));
            var signinModal = driver.FindElement(By.Id("signin-modal"));
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            wait.Until<bool>(d => signinBtn.Displayed);

            jsExecutor.ExecuteScript("$('#signin-login').val(arguments[0])", username);
            jsExecutor.ExecuteScript("$('#signin-password').val(arguments[0])", password);
            signinBtn.Click();

            wait.Until<bool>(d => signinBtn.Enabled || !signinModal.Displayed);

            Assert.IsFalse(signinModal.Displayed);
        }
    }
}
