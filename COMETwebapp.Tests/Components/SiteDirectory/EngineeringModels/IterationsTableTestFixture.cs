// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IterationsTableTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//     Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//   --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Components.SiteDirectory.EngineeringModels
{
    using Bunit;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.SiteDirectory.EngineeringModel;
    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class IterationsTableTestFixture
    {
        private BunitContext context;
        private IRenderedComponent<IterationsTable> renderer;
        private Mock<IIterationsTableViewModel> viewModel;
        private IterationSetup iteration1;
        private IterationSetup iteration2;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.viewModel = new Mock<IIterationsTableViewModel>();

            this.iteration1 = new IterationSetup
            {
                IterationNumber = 1,
                Container = new EngineeringModelSetup { ShortName = "model" }
            };

            this.iteration2 = new IterationSetup
            {
                IterationNumber = 2,
                Container = new EngineeringModelSetup { ShortName = "model" }
            };

            var rows = new SourceList<IterationSetupRowViewModel>();
            rows.Add(new IterationSetupRowViewModel(this.iteration1) { IsAllowedToWrite = true });
            rows.Add(new IterationSetupRowViewModel(this.iteration2) { IsAllowedToWrite = true });

            this.viewModel.Setup(x => x.Rows).Returns(rows);
            this.viewModel.Setup(x => x.CurrentThing).Returns(new IterationSetup());
            this.viewModel.Setup(x => x.CanCreateIteration).Returns(true);
            this.viewModel.Setup(x => x.SourceIterations).Returns(new List<IterationSetup> { this.iteration1, this.iteration2 });

            this.context.Services.AddSingleton<IShowHideDeprecatedThingsService>(new ShowHideDeprecatedThingsService());
            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.ConfigureDevExpressBlazor();

            this.renderer = this.context.Render<IterationsTable>(p => { p.Add(parameter => parameter.ViewModel, this.viewModel.Object); });
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.context.Dispose();
        }

        [Test]
        public async Task VerifyAddingOrEditingIteration()
        {
            var addIterationButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addIterationButton");
            await this.renderer.InvokeAsync(addIterationButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ShouldCreateThing, Is.True);
                Assert.That(this.viewModel.Object.CurrentThing, Is.InstanceOf<IterationSetup>());
            });

            var editIterationButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "editIterationButton");
            await this.renderer.InvokeAsync(editIterationButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ShouldCreateThing, Is.False);
                Assert.That(this.viewModel.Object.CurrentThing, Is.InstanceOf<IterationSetup>());
            });

            var saveIterationButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "saveIterationButton");
            await this.renderer.InvokeAsync(saveIterationButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.CreateOrEditIteration(It.IsAny<bool>()), Times.Once);
        }

        [Test]
        public async Task VerifyDeleteIteration()
        {
            var deleteButtons = this.renderer.FindComponents<DxButton>().Where(x => x.Instance.Id == "deleteIterationButton").ToList();
            
            Assert.Multiple(() =>
            {
                Assert.That(deleteButtons[0].Instance.Enabled, Is.True);
                Assert.That(deleteButtons[1].Instance.Enabled, Is.False);
            });

            await this.renderer.InvokeAsync(deleteButtons[0].Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.OnDeleteButtonClick(It.IsAny<IterationSetupRowViewModel>()), Times.Once);
        }

        [Test]
        public void VerifyOnInitialized()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ShouldCreateThing, Is.False);
                Assert.That(this.renderer.Instance.ViewModel, Is.Not.Null);
                Assert.That(this.renderer.Markup, Does.Contain("Iteration 1"));
                Assert.That(this.renderer.Markup, Does.Contain("Iteration 2"));
            });
        }
    }
}
