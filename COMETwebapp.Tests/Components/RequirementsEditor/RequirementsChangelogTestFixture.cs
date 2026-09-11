// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsChangelogTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.RequirementsEditor
{
    using Bunit;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.RequirementsEditor;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using DevExpress.Blazor;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RequirementsChangelogTestFixture
    {
        private BunitContext context;
        private Mock<IRequirementsChangelogViewModel> viewModel;
        private Mock<IDomDataService> domDataService;
        private IterationSetup frozenSetupOne;
        private IterationSetup frozenSetupTwo;
        private IReadOnlyList<IterationSetup> availableBaselines;
        private IReadOnlyList<RequirementChange> changes;
        private List<RequirementChange> navigatedTo;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.domDataService = new Mock<IDomDataService>();
            this.context.Services.AddSingleton(this.domDataService.Object);

            this.frozenSetupOne = new IterationSetup { Iid = Guid.NewGuid(), IterationNumber = 1, FrozenOn = new DateTime(2026, 1, 1) };
            this.frozenSetupTwo = new IterationSetup { Iid = Guid.NewGuid(), IterationNumber = 2, FrozenOn = new DateTime(2026, 2, 1) };
            this.availableBaselines = [this.frozenSetupOne, this.frozenSetupTwo];

            var specificationOneId = Guid.NewGuid();
            var specificationTwoId = Guid.NewGuid();

            this.changes =
            [
                new RequirementChange { Kind = RequirementChangeKind.Created, ElementKind = "Requirement", ElementId = Guid.NewGuid(), ElementShortName = "R01", SpecificationId = specificationOneId, SpecificationShortName = "KUR", SpecificationName = "Key-User Requirements" },
                new RequirementChange { Kind = RequirementChangeKind.Modified, ElementKind = "Requirement", ElementId = Guid.NewGuid(), ElementShortName = "R02", SpecificationId = specificationOneId, SpecificationShortName = "KUR", SpecificationName = "Key-User Requirements" },
                new RequirementChange { Kind = RequirementChangeKind.Deleted, ElementKind = "Requirement", ElementId = Guid.NewGuid(), ElementShortName = "R03", SpecificationId = specificationTwoId, SpecificationShortName = "SYS", SpecificationName = "System Requirements" },
                new RequirementChange { Kind = RequirementChangeKind.Deprecated, ElementKind = "Requirement", ElementId = Guid.NewGuid(), ElementShortName = "R04", SpecificationId = Guid.Empty }
            ];

            this.navigatedTo = [];

            this.viewModel = new Mock<IRequirementsChangelogViewModel>();
            this.viewModel.SetupGet(x => x.AvailableBaselines).Returns(this.availableBaselines);
            this.viewModel.SetupGet(x => x.SelectedBaseline).Returns(this.frozenSetupTwo);
            this.viewModel.SetupGet(x => x.IsLoading).Returns(false);
            this.viewModel.SetupGet(x => x.Message).Returns(string.Empty);
            this.viewModel.SetupGet(x => x.HasCompared).Returns(true);
            this.viewModel.SetupGet(x => x.Changes).Returns(this.changes);
            this.viewModel.SetupGet(x => x.ScrollToElementId).Returns((Guid?)null);
            this.viewModel.Setup(x => x.CompareAsync()).Returns(Task.CompletedTask);
            this.viewModel.Setup(x => x.ExportAsync(It.IsAny<IReadOnlyList<RequirementChange>>())).Returns(Task.CompletedTask);
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        /// <summary>
        /// Renders the <see cref="RequirementsChangelog" /> with the currently configured <see cref="viewModel" />.
        /// </summary>
        private IRenderedComponent<RequirementsChangelog> Render()
        {
            return this.context.Render<RequirementsChangelog>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.OnNavigateToElement, (RequirementChange change) => this.navigatedTo.Add(change)));
        }

        [Test]
        public void VerifyComparedGridRenders()
        {
            var renderedComponent = this.Render();
            var markup = renderedComponent.Markup;

            Assert.Multiple(() =>
            {
                Assert.That(markup, Does.Contain("Compare"));
                Assert.That(markup, Does.Contain("Export to Excel"));
                Assert.That(markup, Does.Contain("req-changelog-grid"));
                Assert.That(markup, Does.Contain("req-changelog-filters"));
            });
        }

        [Test]
        public void VerifyEmptyBaselinesMessage()
        {
            this.viewModel.SetupGet(x => x.AvailableBaselines).Returns((IReadOnlyList<IterationSetup>)[]);

            var renderedComponent = this.Render();

            Assert.That(renderedComponent.Markup, Does.Contain("no earlier (frozen) iterations"));
        }

        [Test]
        public void VerifyNoChangesMessage()
        {
            this.viewModel.SetupGet(x => x.Changes).Returns((IReadOnlyList<RequirementChange>)[]);

            var renderedComponent = this.Render();

            Assert.That(renderedComponent.Markup, Does.Contain("No requirement changes"));
        }

        [Test]
        public void VerifyErrorMessage()
        {
            this.viewModel.SetupGet(x => x.Message).Returns("boom");

            var renderedComponent = this.Render();

            Assert.That(renderedComponent.Markup, Does.Contain("boom"));
        }

        [Test]
        public async Task VerifyCompareButtonInvokesViewModel()
        {
            var renderedComponent = this.Render();
            var compareButton = renderedComponent.FindComponents<DxButton>().First(x => x.Instance.Text == "Compare");

            await renderedComponent.InvokeAsync(compareButton.Instance.Click.InvokeAsync);

            this.viewModel.Verify(x => x.CompareAsync(), Times.Once);
        }

        [Test]
        public async Task VerifyExportButtonInvokesViewModel()
        {
            var renderedComponent = this.Render();
            var exportButton = renderedComponent.FindComponents<DxButton>().First(x => x.Instance.Text == "Export to Excel");

            await renderedComponent.InvokeAsync(exportButton.Instance.Click.InvokeAsync);

            this.viewModel.Verify(x => x.ExportAsync(It.IsAny<IReadOnlyList<RequirementChange>>()), Times.Once);
        }

        [Test]
        public void VerifyScrollRequestScrollsAndClears()
        {
            var elementId = Guid.NewGuid();
            this.viewModel.SetupGet(x => x.ScrollToElementId).Returns(elementId);

            var renderedComponent = this.Render();

            renderedComponent.WaitForAssertion(() => this.domDataService.Verify(x => x.ScrollElementIntoView($"changelog-row-{elementId}"), Times.AtLeastOnce));

            this.viewModel.Verify(x => x.ClearScrollTarget(), Times.AtLeastOnce);
        }
    }
}
