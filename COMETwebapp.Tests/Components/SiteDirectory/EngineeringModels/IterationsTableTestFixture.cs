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
    using System.Linq;
    using System.Threading.Tasks;

    using Bunit;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Common;
    using COMETwebapp.Components.SiteDirectory.EngineeringModel;
    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components.Forms;
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
            this.viewModel.Setup(x => x.SourceIterations).Returns([this.iteration1, this.iteration2]);

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
        public void VerifyOnInitialized()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.renderer.Instance.ShouldCreateThing, Is.False);
                Assert.That(this.renderer.Instance.IsOnEditMode, Is.False);
                Assert.That(this.renderer.Instance.ViewModel, Is.Not.Null);
                Assert.That(this.renderer.Markup, Does.Contain("Iteration 1"));
                Assert.That(this.renderer.Markup, Does.Contain("Iteration 2"));
            }
        }

        [Test]
        public async Task VerifyDeleteIteration()
        {
            var deleteButtons = this.renderer.FindComponents<DxButton>().Where(x => x.Instance.Id == "deleteIterationButton").ToList();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(deleteButtons[0].Instance.Enabled, Is.True);
                Assert.That(deleteButtons[1].Instance.Enabled, Is.False);
            }

            await this.renderer.InvokeAsync(deleteButtons[0].Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.OnDeleteButtonClick(It.IsAny<IterationSetupRowViewModel>()), Times.Once);
        }

        [Test]
        public async Task VerifyAddingOrEditingIteration()
        {
            var addIterationButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addIterationButton");
            await this.renderer.InvokeAsync(addIterationButton.Instance.Click.InvokeAsync);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.renderer.Instance.ShouldCreateThing, Is.True);
                Assert.That(this.renderer.Instance.IsOnEditMode, Is.True);
                Assert.That(this.viewModel.Object.CurrentThing, Is.InstanceOf<IterationSetup>());
            }

            var editIterationButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "editIterationButton");
            await this.renderer.InvokeAsync(editIterationButton.Instance.Click.InvokeAsync);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.renderer.Instance.ShouldCreateThing, Is.False);
                Assert.That(this.renderer.Instance.IsOnEditMode, Is.True);
                Assert.That(this.viewModel.Object.CurrentThing, Is.InstanceOf<IterationSetup>());
            }

            var iterationsForm = this.renderer.FindComponent<IterationsForm>();
            var editForm = iterationsForm.FindComponent<EditForm>();
            await iterationsForm.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);

            using (Assert.EnterMultipleScope())
            {
                this.viewModel.Verify(x => x.CreateOrEditIteration(false), Times.Once);
                Assert.That(this.renderer.Instance.IsOnEditMode, Is.False);
            }
        }

        [Test]
        public async Task VerifyStartCreate()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.renderer.Instance.ShouldCreateThing, Is.False);
                Assert.That(this.renderer.Instance.IsOnEditMode, Is.False);
            }

            await this.renderer.InvokeAsync(() => this.renderer.Instance.StartCreate());

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.renderer.Instance.ShouldCreateThing, Is.True);
                Assert.That(this.renderer.Instance.IsOnEditMode, Is.True);
            }
        }

        [Test]
        public async Task VerifyStartEdit()
        {
            var row = new IterationSetupRowViewModel(this.iteration1);
            Assert.That(this.renderer.Instance.IsOnEditMode, Is.False);

            await this.renderer.InvokeAsync(() => this.renderer.Instance.StartEdit(null));
            Assert.That(this.renderer.Instance.IsOnEditMode, Is.False);

            await this.renderer.InvokeAsync(() => this.renderer.Instance.StartEdit(row));
            Assert.That(this.renderer.Instance.IsOnEditMode, Is.True);
        }

        [Test]
        public async Task VerifyFormSubmissionAndCancellation()
        {
            var addIterationButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addIterationButton");
            await this.renderer.InvokeAsync(addIterationButton.Instance.Click.InvokeAsync);

            Assert.That(this.renderer.Instance.IsOnEditMode, Is.True);

            var iterationsForm = this.renderer.FindComponent<IterationsForm>();
            var formButtons = iterationsForm.FindComponent<FormButtons>();
            await iterationsForm.InvokeAsync(formButtons.Instance.OnCancel.InvokeAsync);

            Assert.That(this.renderer.Instance.IsOnEditMode, Is.False);

            await this.renderer.InvokeAsync(addIterationButton.Instance.Click.InvokeAsync);
            Assert.That(this.renderer.Instance.IsOnEditMode, Is.True);

            iterationsForm = this.renderer.FindComponent<IterationsForm>();
            var editForm = iterationsForm.FindComponent<EditForm>();
            await iterationsForm.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);

            using (Assert.EnterMultipleScope())
            {
                this.viewModel.Verify(x => x.CreateOrEditIteration(true), Times.Once);
                Assert.That(this.renderer.Instance.IsOnEditMode, Is.False);
            }
        }
    }
}
