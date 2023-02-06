// -------------------------------------------------------------------------------------------------------------------- 
// <copyright file="ConfirmChangeSelectionPopUpTestFixture.cs" company="RHEA System S.A."> 
//    Copyright (c) 2023 RHEA System S.A. 
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar 
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

namespace COMETwebapp.Tests.Components.Viewer.PopUps
{
    using Bunit;

    using COMETwebapp.Components.Viewer.PopUps;

    using Microsoft.Extensions.DependencyInjection;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ConfirmChangeSelectionPopUpTestFixture
    {
        private TestContext context;
        private IRenderedComponent<ConfirmChangeSelectionPopUp> renderedComponent;
        private ConfirmChangeSelectionPopUp popUp;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.Services.AddDevExpressBlazor();
            this.renderedComponent = this.context.RenderComponent<ConfirmChangeSelectionPopUp>();
            this.popUp = renderedComponent.Instance;
        }

        [Test]
        public void VerifyThatShowWorks()
        {
            Assert.That(this.popUp.IsVisible, Is.False);
            this.popUp.Show();
            Assert.That(this.popUp.IsVisible, Is.True);
        }

        [Test]
        public void VerifyThatHideWorks()
        {
            this.popUp.Show();
            Assert.That(this.popUp.IsVisible, Is.True);
            this.popUp.Hide();
            Assert.That(this.popUp.IsVisible, Is.False);
        }

        [Test]
        public void VerifyThatContinueButtonWorks()
        {
            this.popUp.Show();
            
            this.popUp.OnResponse += (sender, response) =>
            {
                Assert.That(response, Is.True);
            };
            
            var button = this.renderedComponent.Find(".continue-button");
            Assert.That(button, Is.Not.Null);
            button.Click();
        }

        [Test]
        public void VerifyThatCancelButtonWorks()
        {
            this.popUp.Show();
            
            this.popUp.OnResponse += (sender, response) =>
            {
                Assert.That(response, Is.False);
            };
            
            var button = this.renderedComponent.Find(".cancel-button");
            Assert.That(button, Is.Not.Null);
            button.Click();
        }
    }
}
