// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoginTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
//
//    Author: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.IntegrationTests
{
    using System;

    using NUnit.Framework;

    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Support.UI;

    using SeleniumExtras.WaitHelpers;

    /// <summary>
    /// Integration test suite that verifies the login feature
    /// </summary>
    [TestFixture, Category("Integration")]
    public class LoginTestFixture
    {
        private string appURL;

        private IWebDriver driver;
        
        [SetUp]        
        public void SetUp()
        {
            this.appURL = "https://localhost:5136";

            var options = new ChromeOptions();
            options.AddArgument("--headless");

            this.driver = new ChromeDriver(options);
            this.driver.Manage().Window.Maximize();
        }

        [TearDown]
        public void TearDown()
        {
            this.driver.Quit();
            this.driver.Dispose();
        }

        [Test]
        public void Verify_that_a_user_can_login()
        {
            driver.Navigate().GoToUrl(appURL + "/");

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            wait.Until(ExpectedConditions.ElementExists(By.Id("unauthorized-notice")));

            Assert.That(driver.Title, Is.EqualTo("COMET Community Edition"));
            Assert.That(driver.FindElement(By.Id("comet-logo")), Is.Not.Null);

            var unauthorizedNotice = driver.FindElement(By.Id("unauthorized-notice"));
            Assert.That(unauthorizedNotice.Text, Is.EqualTo("Connect and Open a model."));


            // login
            driver.FindElement(By.Id("sourceaddress")).SendKeys("https://cdp4services-public.cdp4.org");
            driver.FindElement(By.Id("username")).SendKeys("admin");
            driver.FindElement(By.Id("password")).SendKeys("pass");
            driver.FindElement(By.Id("connectbtn")).Click();

            wait.Until(ExpectedConditions.ElementExists(By.Id("welcome-user-notice")));
            var welcomeUserNotice = driver.FindElement(By.Id("welcome-user-notice"));

            Assert.That(welcomeUserNotice.Text, Is.EqualTo("The Administrator - https://cdp4services-public.cdp4.org/"));            
        }
    }
}
