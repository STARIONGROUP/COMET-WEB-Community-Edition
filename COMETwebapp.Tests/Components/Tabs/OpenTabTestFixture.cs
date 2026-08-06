// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OpenTabTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
//
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.Components.Tabs
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.StringTableService;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components;

    using COMETwebapp.Components.BookEditor;
    using COMETwebapp.Components.ModelDashboard;
    using COMETwebapp.Components.Tabs;
    using COMETwebapp.Model;
    using COMETwebapp.ViewModels.Components.Common.OpenTab;
    using COMETwebapp.ViewModels.Pages;

    using DevExpress.Blazor;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class OpenTabTestFixture
    {
        private BunitContext context;
        private IRenderedComponent<OpenTab> renderer;
        private Mock<IOpenTabViewModel> viewModel;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.viewModel = new Mock<IOpenTabViewModel>();
            this.viewModel.Setup(x => x.IsCurrentIterationOpened).Returns(true);

            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.Services.AddSingleton(new Mock<IOpenModelViewModel>().Object);
            this.context.Services.AddSingleton(new Mock<IStringTableService>().Object);

            this.renderer = this.context.Render<OpenTab>();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.context.Dispose();
        }

        [Test]
        public async Task VerifyOnCancel()
        {
            var wasCanceled = false;
            var closeButton = this.renderer.FindComponents<DxButton>().FirstOrDefault(x => x.Instance.Id == "closetab__button");
            Assert.That(closeButton, Is.Null);

            this.renderer.Render(parameters => { parameters.Add(p => p.OnCancel, () => wasCanceled = true); });

            closeButton = this.renderer.FindComponents<DxButton>().FirstOrDefault(x => x.Instance.Id == "closetab__button");
            Assert.That(closeButton, Is.Not.Null);

            await this.renderer.InvokeAsync(closeButton.Instance.Click.InvokeAsync);
            Assert.That(wasCanceled, Is.True);
        }

        [Test]
        public void VerifyReadinessMarkerIsPublished()
        {
            var form = this.renderer.Find("#open-tab-form");
            Assert.That(form.GetAttribute("data-app-ready"), Is.EqualTo("true"));
        }

        [Test]
        public void VerifyComboItemTemplatesExposeTheTestId()
        {
            var application = Applications.ExistingApplications.OfType<TabbedApplication>().First();
            var modelSetup = new EngineeringModelSetup { Name = "Model" };
            var domain = new DomainOfExpertise { Name = "Domain" };
            var iterationData = new IterationData(new IterationSetup { IterationNumber = 1 });

            // The view, model and domain combos render while no application is selected (the SetUp default). The domain
            // template's highlighted branch is exercised here because IsCurrentIterationOpened is true in the SetUp.
            Assert.Multiple(() =>
            {
                Assert.That(this.RenderComboItemTemplate<TabbedApplication>("view-selection", application), Is.EqualTo(application.Name));
                Assert.That(this.RenderComboItemTemplate<EngineeringModelSetup>("model-selection", modelSetup), Is.EqualTo("Model"));
                Assert.That(this.RenderComboItemTemplate<DomainOfExpertise>("domain-selection", domain), Is.EqualTo("Domain"));
            });

            // The plain branch of the domain template.
            this.viewModel.Setup(x => x.IsCurrentIterationOpened).Returns(false);
            this.renderer.Render();
            Assert.That(this.RenderComboItemTemplate<DomainOfExpertise>("domain-selection", domain), Is.EqualTo("Domain"));

            // The iteration combo only renders for an iteration view.
            var iterationApplication = Applications.ExistingApplications.OfType<TabbedApplication>().First(x => x.ThingTypeOfInterest == typeof(Iteration));
            this.viewModel.Setup(x => x.SelectedApplication).Returns(iterationApplication);
            this.renderer.Render();
            Assert.That(this.RenderComboItemTemplate<IterationData>("iteration-selection", iterationData), Is.EqualTo(iterationData.IterationName));
        }

        /// <summary>
        /// Renders a combo box's <c>ItemTemplate</c> for the given item and returns the text of its application-owned
        /// <c>data-testid</c> marker. bunit does not render the DevExpress drop-down list itself, so the template render
        /// fragment is invoked directly.
        /// </summary>
        /// <typeparam name="T">The combo box data type.</typeparam>
        /// <param name="comboId">The combo box component id.</param>
        /// <param name="item">The item to render the template for.</param>
        /// <returns>The trimmed text of the rendered item's marker element.</returns>
        private string RenderComboItemTemplate<T>(string comboId, T item)
        {
            var combo = this.renderer.FindComponents<DxComboBox<T, T>>().First(x => x.Instance.Id == comboId);
            var renderedItem = this.context.Render(combo.Instance.ItemTemplate(item));
            return renderedItem.Find("[data-testid=combo-item]").TextContent.Trim();
        }

        [Test]
        public void VerifyOnInitialized()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ViewModel, Is.EqualTo(this.viewModel.Object));
                this.viewModel.Verify(x => x.InitializesProperties(), Times.Once);
            });
        }

        [Test]
        public async Task VerifyOpenButton()
        {
            var panel = new TabPanelInformation();

            var openButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "opentab__button");
            await this.renderer.InvokeAsync(openButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.OpenTab(It.IsAny<TabPanelInformation>()), Times.Once);

            var bookEditorBodyComponent = new TabbedApplication
            {
                ComponentType = typeof(BookEditorBody)
            };

            bookEditorBodyComponent.ResolveTypesProperties();

            this.viewModel.Setup(x => x.SelectedApplication).Returns(bookEditorBodyComponent);
            await this.renderer.InvokeAsync(openButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.OpenTab(It.IsAny<TabPanelInformation>()), Times.Exactly(2));

            var modelDashboardBodyComponent = new TabbedApplication
            {
                ComponentType = typeof(ModelDashboardBody)
            };

            modelDashboardBodyComponent.ResolveTypesProperties();

            this.viewModel.Setup(x => x.SelectedApplication).Returns(modelDashboardBodyComponent);
            this.renderer.Render();
            await this.renderer.InvokeAsync(openButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.OpenTab(It.IsAny<TabPanelInformation>()), Times.Exactly(3));
        }

        [Test]
        public void VerifyHeaderTexts()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.HeaderText, Is.EqualTo("You have no model selected"));
                Assert.That(this.renderer.Instance.SubtitleText, Is.EqualTo("Select a model to start working on it"));
            });

            this.viewModel.Setup(x => x.SelectedEngineeringModel).Returns(new EngineeringModelSetup { Name = "Model" });
            this.renderer.Render();

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.HeaderText, Is.EqualTo("Model selected"));
                Assert.That(this.renderer.Instance.SubtitleText, Is.EqualTo("Select options to start working on it"));
            });
        }

        [Test]
        public async Task VerifyGoToOpenTab()
        {
            var tabOpenedCalled = false;
            this.renderer.Render(parameters => { parameters.Add(p => p.OnTabOpened, () => tabOpenedCalled = true); });

            this.viewModel.Setup(x => x.HasOpenTab).Returns(true);
            this.viewModel.Setup(x => x.NavigateToOpenTab()).Returns(true);

            this.renderer.Render();

            Assert.That(this.renderer.Instance.ViewModel.HasOpenTab, Is.True);

            var alreadyOpenButton = this.renderer.FindComponents<DxButton>().FirstOrDefault(x => x.Instance.Id == "already-open-tab-button");
            Assert.That(alreadyOpenButton, Is.Not.Null);

            await this.renderer.InvokeAsync(alreadyOpenButton.Instance.Click.InvokeAsync);

            using (Assert.EnterMultipleScope())
            {
                this.viewModel.Verify(x => x.NavigateToOpenTab(), Times.Once);
                Assert.That(tabOpenedCalled, Is.True);
            }
        }
    }
}
