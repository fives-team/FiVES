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
    }
}
