// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditotPopupTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.Components.BookEditor
{
    using Bunit;

    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Components.BookEditor;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components.BookEditor;

    using DevExpress.Blazor;
    using DevExpress.Blazor.Popup.Internal;

    using DynamicData;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class EditotPopupTestFixture
    {
        private TestContext context;
        private Mock<IEditorPopupViewModel> viewmodel;
        private IRenderedComponent<EditorPopup> component;
        private Book book;
        private List<DomainOfExpertise> activeDomains;
        private List<Category> availableCategories;
        private bool onCancelCalled;
        private bool onAcceptCalled;
        private Mock<ISessionService> sessionService;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();
            this.sessionService = new Mock<ISessionService>();
            this.context.Services.AddSingleton(this.sessionService.Object);

            this.book = new Book();

            this.activeDomains = new List<DomainOfExpertise>
            {
                new()
                {
                    Name = "Sys"
                }
            };

            this.availableCategories = new List<Category>
            {
                new()
                {
                    Name = "Category"
                }
            };

            var onCancelClicked = new EventCallbackFactory().Create(this, () => this.onCancelCalled = true);
            var onConfirmClicked = new EventCallbackFactory().Create(this, () => this.onAcceptCalled = true);

            this.viewmodel = new Mock<IEditorPopupViewModel>();
            this.viewmodel.Setup(x => x.IsVisible).Returns(true);
            this.viewmodel.Setup(x => x.ActiveDomains).Returns(this.activeDomains);
            this.viewmodel.Setup(x => x.AvailableCategories).Returns(this.availableCategories);
            this.viewmodel.Setup(x => x.HeaderText).Returns("Header");
            this.viewmodel.Setup(x => x.OnCancelClick).Returns(onCancelClicked);
            this.viewmodel.Setup(x => x.OnConfirmClick).Returns(onConfirmClicked);
            this.viewmodel.Setup(x => x.ValidationErrors).Returns(new SourceList<string>());

            this.component = this.context.RenderComponent<EditorPopup>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewmodel.Object);
            });
        }
        
        [Test]
        public void VerifyComponent()
        {
            var popup = this.component.FindComponent<DxPopup>();

            Assert.Multiple(() =>
            {
                Assert.That(popup.Instance.Visible, Is.True);
                Assert.That(popup.Instance.HeaderText, Is.EqualTo("Header"));
            });

            var okButton = this.component.Find(".ok-button");
            okButton.Click();

            Assert.That(this.onAcceptCalled, Is.True);

            var cancelButton = this.component.Find(".cancel-button");
            cancelButton.Click();

            Assert.That(this.onCancelCalled, Is.True);

            var errors = new SourceList<string>();
            errors.Add("Error 1");
            errors.Add("Error 2");

            this.viewmodel.Setup(x => x.ValidationErrors).Returns(errors);

            this.component.Render();

            var errorMessages = this.component.FindComponents<ValidationMessageComponent>();

            Assert.Multiple(() =>
            {
                Assert.That(errorMessages, Has.Count.EqualTo(2));
                Assert.That(errorMessages[0].Instance.ValidationMessage, Is.EqualTo("Error 1"));
                Assert.That(errorMessages[1].Instance.ValidationMessage, Is.EqualTo("Error 2"));
            });
        }
    }
}
