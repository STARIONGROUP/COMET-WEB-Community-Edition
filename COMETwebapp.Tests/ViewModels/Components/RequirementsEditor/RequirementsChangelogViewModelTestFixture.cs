// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsChangelogViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.RequirementsEditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Services.Export;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using DynamicData;

    using FluentResults;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RequirementsChangelogViewModelTestFixture
    {
        private RequirementsChangelogViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IExportService> exportService;
        private SourceList<Iteration> openIterations;
        private DomainOfExpertise domain;
        private EngineeringModelSetup modelSetup;
        private IterationSetup currentSetup;
        private IterationSetup frozenSetup2;
        private IterationSetup frozenSetup1;
        private Iteration currentIteration;
        private Iteration baselineIteration;

        [SetUp]
        public void SetUp()
        {
            this.domain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System Engineering" };

            this.currentSetup = new IterationSetup { Iid = Guid.NewGuid(), IterationNumber = 3, FrozenOn = null };
            this.frozenSetup2 = new IterationSetup { Iid = Guid.NewGuid(), IterationNumber = 2, FrozenOn = new DateTime(2026, 1, 1) };
            this.frozenSetup1 = new IterationSetup { Iid = Guid.NewGuid(), IterationNumber = 1, FrozenOn = new DateTime(2026, 1, 1) };

            this.modelSetup = new EngineeringModelSetup { Iid = Guid.NewGuid(), ShortName = "MODEL" };
            this.modelSetup.IterationSetup.AddRange([this.currentSetup, this.frozenSetup2, this.frozenSetup1]);

            var currentSpecification = new RequirementsSpecification { Iid = Guid.NewGuid(), ShortName = "KUR", Name = "Key-User Requirements", Owner = this.domain };
            var requirementIid = Guid.NewGuid();
            currentSpecification.Requirement.Add(new Requirement { Iid = requirementIid, ShortName = "R01", Name = "New name", Owner = this.domain });
            this.currentIteration = new Iteration { Iid = Guid.NewGuid(), IterationSetup = this.currentSetup };
            this.currentIteration.RequirementsSpecification.Add(currentSpecification);

            var baselineSpecification = new RequirementsSpecification { Iid = currentSpecification.Iid, ShortName = "KUR", Name = "Key-User Requirements", Owner = this.domain };
            baselineSpecification.Requirement.Add(new Requirement { Iid = requirementIid, ShortName = "R01", Name = "Old name", Owner = this.domain });
            this.baselineIteration = new Iteration { Iid = Guid.NewGuid(), IterationSetup = this.frozenSetup2 };
            this.baselineIteration.RequirementsSpecification.Add(baselineSpecification);

            this.openIterations = new SourceList<Iteration>();
            this.sessionService = new Mock<ISessionService>();
            this.sessionService.Setup(x => x.OpenIterations).Returns(this.openIterations);

            this.exportService = new Mock<IExportService>();
            this.exportService.Setup(x => x.ExportAndDownloadAsync(It.IsAny<IExporter>())).Returns(Task.CompletedTask);
            this.viewModel = new RequirementsChangelogViewModel(this.sessionService.Object, this.exportService.Object, new Mock<ILogger>().Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.viewModel.Dispose();
            this.openIterations.Dispose();
        }

        [Test]
        public void VerifySetIteration()
        {
            this.viewModel.SetIteration(this.currentIteration, this.domain);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableBaselines, Has.Count.EqualTo(2));
                Assert.That(this.viewModel.AvailableBaselines[0].IterationNumber, Is.EqualTo(2));
                Assert.That(this.viewModel.AvailableBaselines[1].IterationNumber, Is.EqualTo(1));
                Assert.That(this.viewModel.SelectedBaseline.IterationNumber, Is.EqualTo(2));
                Assert.That(this.viewModel.Changes, Is.Empty);
                Assert.That(this.viewModel.HasCompared, Is.False);
                Assert.That(this.viewModel.CurrentIteration, Is.EqualTo(this.currentIteration));
                Assert.That(this.viewModel.CurrentDomain, Is.EqualTo(this.domain));
            });

            this.viewModel.SetIteration(null, this.domain);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableBaselines, Is.Empty);
                Assert.That(this.viewModel.SelectedBaseline, Is.Null);
            });
        }

        [Test]
        public async Task VerifyCompareAsyncOpensAndClosesBaseline()
        {
            this.sessionService.Setup(x => x.ReadIteration(It.IsAny<IterationSetup>(), It.IsAny<DomainOfExpertise>())).Returns(Task.FromResult(Result.Ok(this.baselineIteration)));
            this.sessionService.Setup(x => x.CloseIteration(It.IsAny<Iteration>())).Returns(Task.CompletedTask);

            this.viewModel.SetIteration(this.currentIteration, this.domain);
            await this.viewModel.CompareAsync();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Changes, Is.Not.Empty);
                Assert.That(this.viewModel.Message, Is.Empty);
                Assert.That(this.viewModel.HasCompared, Is.True);
            });

            this.sessionService.Verify(x => x.ReadIteration(It.IsAny<IterationSetup>(), It.IsAny<DomainOfExpertise>()), Times.Once);
            this.sessionService.Verify(x => x.CloseIteration(this.baselineIteration), Times.Once);
        }

        [Test]
        public async Task VerifyCompareAsyncReusesOpenBaseline()
        {
            this.openIterations.Add(this.baselineIteration);

            this.viewModel.SetIteration(this.currentIteration, this.domain);
            await this.viewModel.CompareAsync();

            Assert.That(this.viewModel.Changes, Is.Not.Empty);

            this.sessionService.Verify(x => x.ReadIteration(It.IsAny<IterationSetup>(), It.IsAny<DomainOfExpertise>()), Times.Never);
            this.sessionService.Verify(x => x.CloseIteration(It.IsAny<Iteration>()), Times.Never);
        }

        [Test]
        public async Task VerifyCompareAsyncHandlesReadFailure()
        {
            this.sessionService.Setup(x => x.ReadIteration(It.IsAny<IterationSetup>(), It.IsAny<DomainOfExpertise>())).Returns(Task.FromResult(Result.Fail<Iteration>("boom")));

            this.viewModel.SetIteration(this.currentIteration, this.domain);
            await this.viewModel.CompareAsync();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Message, Is.Not.Empty);
                Assert.That(this.viewModel.Changes, Is.Empty);
                Assert.That(this.viewModel.HasCompared, Is.True);
            });

            this.sessionService.Verify(x => x.CloseIteration(It.IsAny<Iteration>()), Times.Never);
        }

        [Test]
        public async Task VerifyCompareAsyncWithoutSelectionDoesNothing()
        {
            await this.viewModel.CompareAsync();

            this.sessionService.Verify(x => x.ReadIteration(It.IsAny<IterationSetup>(), It.IsAny<DomainOfExpertise>()), Times.Never);
        }

        [Test]
        public async Task VerifyExportAsync()
        {
            await this.viewModel.ExportAsync(null);
            await this.viewModel.ExportAsync([]);

            this.exportService.Verify(x => x.ExportAndDownloadAsync(It.IsAny<IExporter>()), Times.Never);

            RequirementChange[] changes = [new RequirementChange { Kind = RequirementChangeKind.Created, ElementKind = "Requirement", ElementShortName = "R01", SpecificationId = Guid.NewGuid() }];

            await this.viewModel.ExportAsync(changes);

            this.exportService.Verify(x => x.ExportAndDownloadAsync(It.IsAny<IExporter>()), Times.Once);
        }

        [Test]
        public async Task VerifyExportAsyncSetsMessageOnFailure()
        {
            this.exportService.Setup(x => x.ExportAndDownloadAsync(It.IsAny<IExporter>())).ThrowsAsync(new InvalidOperationException("boom"));

            RequirementChange[] changes = [new RequirementChange { Kind = RequirementChangeKind.Created, ElementKind = "Requirement", ElementShortName = "R01", SpecificationId = Guid.NewGuid() }];

            await this.viewModel.ExportAsync(changes);

            Assert.That(this.viewModel.Message, Is.Not.Empty, "a failed export must surface a message to the user.");
        }

        [Test]
        public void VerifyRequestScrollTo()
        {
            var elementId = Guid.NewGuid();

            this.viewModel.RequestScrollTo(elementId);
            var scrollToElementIdAfterRequest = this.viewModel.ScrollToElementId;

            this.viewModel.ClearScrollTarget();
            var scrollToElementIdAfterClear = this.viewModel.ScrollToElementId;

            Assert.Multiple(() =>
            {
                Assert.That(scrollToElementIdAfterRequest, Is.EqualTo(elementId));
                Assert.That(scrollToElementIdAfterClear, Is.Null);
            });
        }
    }
}
