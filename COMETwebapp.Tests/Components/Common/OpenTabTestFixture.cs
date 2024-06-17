// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OpenTabTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.Tests.Components.Common
{
    using Bunit;

    using COMET.Web.Common.Services.StringTableService;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components;

    using COMETwebapp.Components.BookEditor;
    using COMETwebapp.Components.Common;
    using COMETwebapp.Components.ModelDashboard;
    using COMETwebapp.Model;
    using COMETwebapp.ViewModels.Components.Common.OpenTab;

    using DevExpress.Blazor;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class OpenTabTestFixture
    {
        private TestContext context;
        private IRenderedComponent<OpenTab> renderer;
        private Mock<IOpenTabViewModel> viewModel;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            this.viewModel = new Mock<IOpenTabViewModel>();
            this.viewModel.Setup(x => x.IsCurrentModelOpened).Returns(true);

            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.Services.AddSingleton(new Mock<IOpenModelViewModel>().Object);
            this.context.Services.AddSingleton(new Mock<IStringTableService>().Object);

            this.renderer = this.context.RenderComponent<OpenTab>();
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

            this.renderer.SetParametersAndRender(parameters => { parameters.Add(p => p.OnCancel, () => wasCanceled = true); });

            closeButton = this.renderer.FindComponents<DxButton>().FirstOrDefault(x => x.Instance.Id == "closetab__button");
            Assert.That(closeButton, Is.Not.Null);

            await this.renderer.InvokeAsync(closeButton.Instance.Click.InvokeAsync);
            Assert.That(wasCanceled, Is.True);
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
            var openButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "opentab__button");
            await this.renderer.InvokeAsync(openButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.OpenSession(), Times.Once);

            var bookEditorBodyComponent = new TabbedApplication
            {
                ComponentType = typeof(BookEditorBody)
            };

            bookEditorBodyComponent.ResolveTypesProperties();

            this.viewModel.Setup(x => x.SelectedApplication).Returns(bookEditorBodyComponent);
            await this.renderer.InvokeAsync(openButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.OpenSession(), Times.Exactly(2));

            var modelDashboardBodyComponent = new TabbedApplication
            {
                ComponentType = typeof(ModelDashboardBody)
            };

            modelDashboardBodyComponent.ResolveTypesProperties();

            this.viewModel.Setup(x => x.SelectedApplication).Returns(modelDashboardBodyComponent);
            this.renderer.Render();
            await this.renderer.InvokeAsync(openButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.OpenSession(), Times.Exactly(3));
        }
    }
}
