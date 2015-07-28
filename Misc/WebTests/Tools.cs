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
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;
using TestingPlugin;
using System.ServiceModel;
using System.Threading;

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

        public static IWebDriver CreateDriver()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--disable-cache");
            var driver = new ChromeDriver(options);
            return driver;
        }
    }
}
