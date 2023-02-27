// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IntegrationTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft, Antoine Théate
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
    using System.Linq;

    using COMETwebapp.Extensions;
    using COMETwebapp.Utilities;

    using NUnit.Framework;

    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Support.UI;

    using SeleniumExtras.WaitHelpers;

    /// <summary>
    /// Integration test suite that verifies the login feature
    /// </summary>
    [TestFixture]
    [Category("Integration")]
    public class IntegrationTestFixture
    {
        private string appUrl;
        private IWebDriver driver;
        private string targetServerUrl;
        private WebDriverWait wait;

		[SetUp]
        public void SetUp()
        {
            this.appUrl = "https://localhost:5136";
            this.targetServerUrl = "http://localhost:5000";

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
            this.driver.Navigate().GoToUrl(this.appUrl + "/");

            this.wait = new WebDriverWait(this.driver, TimeSpan.FromSeconds(120));

            this.wait.Until(ExpectedConditions.ElementExists(By.Id("unauthorized-notice")));

            Assert.That(this.driver.Title, Is.EqualTo("COMET Community Edition"));
            Assert.That(this.driver.FindElement(By.Id("comet-logo")), Is.Not.Null);

            var unauthorizedNotice = this.driver.FindElement(By.Id("unauthorized-notice"));
            Assert.That(unauthorizedNotice.Text, Is.EqualTo("Connect and Open a Model."));

            // login

            this.driver.FindElement(By.Id("sourceaddress")).SendKeys(this.targetServerUrl);
            this.driver.FindElement(By.Id("username")).SendKeys("admin");
            this.driver.FindElement(By.Id("password")).SendKeys("pass");
            this.driver.FindElement(By.Id("connectbtn")).Click();

            this.wait.Until(ExpectedConditions.ElementExists(By.Id("welcome-user-notice")));
        }

        [Test]
        public void VerifyCanOpenModel()
        {
            this.Verify_that_a_user_can_login();

            var welcomeUserNotice = this.driver.FindElement(By.Id("welcome-user-notice"));

            Assert.That(welcomeUserNotice.Text, Is.EqualTo($"The Administrator - {this.targetServerUrl}/"));

            var openModelButton = this.driver.FindElement(By.Id("openmodel__button"));
            Assert.That(openModelButton.Enabled, Is.False);

            var modelSelection = this.driver.FindElement(By.Name("model-selection"));
            modelSelection.Click();

            var listBoxItems = this.driver.FindElements(By.ClassName("dxbl-listbox-item"));
            Assert.That(listBoxItems, Has.Count.GreaterThanOrEqualTo(1));

            listBoxItems.First().Click();
            Assert.That(openModelButton.Enabled, Is.False);

            this.wait.Until(ExpectedConditions.ElementExists(By.Id("iteration-selection")));
            var iterationSelection = this.driver.FindElement(By.Name("iteration-selection"));
            iterationSelection.Click();
            Assert.That(openModelButton.Enabled, Is.False);

            listBoxItems = this.driver.FindElements(By.ClassName("dxbl-listbox-item"));
            Assert.That(listBoxItems, Has.Count.GreaterThanOrEqualTo(1));
            listBoxItems.First().Click();
            Assert.That(openModelButton.Enabled, Is.False);

            var domainSelection = this.driver.FindElement(By.Name("domain-selection"));
            domainSelection.Click();

            listBoxItems = this.driver.FindElements(By.ClassName("dxbl-listbox-item"));
            Assert.That(listBoxItems, Has.Count.GreaterThanOrEqualTo(1));
            listBoxItems.First().Click();
            Assert.That(openModelButton.Enabled, Is.True);
            openModelButton.Click();

            this.wait.Until((x) => x.FindElements(By.Id("openmodel__button")).Count == 0);
        }

        [Test]
        public void VerifyCanRefreshSession()
        {
            this.VerifyCanOpenModel();
            var sessionEntry = this.GetMenuItem("session-entry");
            Assert.That(sessionEntry, Is.Not.Null);
            sessionEntry.Click();
            this.wait.Until(ExpectedConditions.ElementExists(By.Id("refresh-button")));
            var refreshButton = this.driver.FindElement(By.Id("refresh-button"));
            
            Assert.Multiple(() =>
            {
                Assert.That(refreshButton.Enabled, Is.True);
                Assert.That(refreshButton.Text, Is.EqualTo("Refresh"));
            });

            refreshButton.Click();

            Assert.Multiple(() =>
            {
                Assert.That(refreshButton.Enabled, Is.False);
                Assert.That(refreshButton.Text, Is.EqualTo("Refreshing"));
            });

            this.wait.Until(_ => refreshButton.Enabled);
        }

        [Test]
        public void VerifyCanAutoResfreshSession()
        {
            this.VerifyCanOpenModel();
            var sessionEntry = this.GetMenuItem("session-entry");
            Assert.That(sessionEntry, Is.Not.Null);
            sessionEntry.Click();

            this.wait.Until(ExpectedConditions.ElementExists(By.Id("autorefresh-value")));
            this.driver.FindElement(By.Id("autorefresh-value")).SendKeys("5");
            var refreshButton = this.driver.FindElement(By.Id("refresh-button"));
            this.driver.FindElement(By.Id("autorefresh-check")).Click();
            this.wait.Until(_ => !refreshButton.Enabled);
            Assert.That(refreshButton.Text, Is.EqualTo("Refreshing"));
            this.wait.Until(_ => refreshButton.Enabled);
            Assert.That(refreshButton.Text, Is.EqualTo("Refresh"));
        }

        [Test]
        public void VerifyCanLogout()
        {
            this.VerifyCanOpenModel();
            var sessionEntry = this.GetMenuItem("session-entry");
            Assert.That(sessionEntry, Is.Not.Null);
            sessionEntry.Click();

            this.wait.Until(ExpectedConditions.ElementExists(By.Id("logout-button")));
            this.driver.FindElement(By.Id("logout-button")).Click();
            this.wait.Until(ExpectedConditions.ElementExists(By.Id("unauthorized-notice")));
        }

        [Test]
        public void VerifyCanSwitchDomain()
        {
            this.VerifyCanOpenModel();
            var modelEntry = this.GetMenuItem("model-entry");
            Assert.That(modelEntry, Is.Not.Null);
            modelEntry.Click();

            this.wait.Until(ExpectedConditions.ElementExists(By.Id("model-entry-row-0")));
            this.driver.FindElement(By.Id("model-entry-row-0")).Click();
            this.wait.Until(ExpectedConditions.ElementExists(By.Id("model-entry-row-0-switch")));
            this.driver.FindElement(By.Id("model-entry-row-0-switch")).Click();
            this.wait.Until(ExpectedConditions.ElementExists(By.Id("switch-domain")));
            var button = this.driver.FindElement(By.Id("switch-domain-button"));
            Assert.That(button.Enabled, Is.True);
            button.Click();
            Assert.That(() => this.driver.FindElement(By.Id("switch-domain")), Throws.Exception);
        }

        [TestCase(ConstantValues.ModelDashboardPage)]
        [TestCase(ConstantValues.ParameterEditorPage)]
        [TestCase(ConstantValues.SubscriptionDashboardPage)]
        [TestCase(ConstantValues.ViewerPage)]
        public void VerifyNavigateToApplication(string pageName)
        {
            this.VerifyCanOpenModel();
            var card = this.driver.FindElement(By.Id($"{pageName.ToLower()}"));
            card.Click();
            Assert.That(this.driver.Url, Does.Contain($"{pageName}"));
            this.wait.Until(ExpectedConditions.ElementExists(By.Id(pageName.QueryPageBodyName())));
        }

        private IWebElement GetMenuItem(string id)
        {
            return this.driver.FindElement(By.Id(id)).FindElement(By.XPath("./../.."));
        }
    }
}
