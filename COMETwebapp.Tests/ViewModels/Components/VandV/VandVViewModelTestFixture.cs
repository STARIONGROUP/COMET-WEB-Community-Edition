// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.VandV
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Utilities.VandV;
    using COMETwebapp.ViewModels.Components.VandV;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class VandVViewModelTestFixture
    {
        private VandVViewModel viewModel;
        private VandVRegisterScenario scenario;

        [SetUp]
        public void SetUp()
        {
            var permissionService = new Mock<IPermissionService>();
            permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(permissionService.Object);
            var sessionService = new Mock<ISessionService>();
            sessionService.Setup(x => x.Session).Returns(session.Object);

            this.scenario = new VandVRegisterScenario();
            this.viewModel = new VandVViewModel(sessionService.Object, new Mock<ILogger>().Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifySetIteration()
        {
            var initial = (this.viewModel.Model.Iteration, this.viewModel.ShowEmptyState);
            this.viewModel.SetIteration(this.scenario.Data.Iteration);

            Assert.Multiple(() =>
            {
                Assert.That(initial.Iteration, Is.Null);
                Assert.That(initial.ShowEmptyState, Is.False, "no model library, so nothing to set up");
                Assert.That(this.viewModel.Model.Iteration, Is.SameAs(this.scenario.Data.Iteration));
                Assert.That(this.viewModel.Model.Items, Has.Count.EqualTo(3));
                Assert.That(this.viewModel.RegisterViewModel.Model, Is.SameAs(this.viewModel.Model), "the register reads the same snapshot");
                Assert.That(this.viewModel.RegisterViewModel.Rows, Is.Not.Empty);
            });

            this.viewModel.SetIteration(null);

            Assert.That(this.viewModel.RegisterViewModel.Rows, Is.Empty);

            var stage = (EnumerationParameterType)this.scenario.Data.ParameterTypes[VandVConstants.Stage];
            stage.ValueDefinition.Clear();
            stage.ValueDefinition.Add(new EnumerationValueDefinition { Iid = Guid.NewGuid(), Name = "MDR", ShortName = "MDR" });
            this.AttachLibrary(this.scenario.Data.Library);

            this.viewModel.SetIteration(this.scenario.Data.Iteration);

            Assert.That(this.viewModel.Model.StageGates[0], Is.EqualTo("MDR"), "the gates of the model library's chain, in project order");
        }

        [Test]
        public void VerifyLoadError()
        {
            var permissionService = new Mock<IPermissionService>();
            permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Throws(new InvalidOperationException("broken"));
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(permissionService.Object);
            var sessionService = new Mock<ISessionService>();
            sessionService.Setup(x => x.Session).Returns(session.Object);
            this.AttachLibrary(this.scenario.Data.Library);

            using var failing = new VandVViewModel(sessionService.Object, new Mock<ILogger>().Object);
            var initial = failing.LoadError;

            Assert.That(() => failing.SetIteration(this.scenario.Data.Iteration), Throws.Nothing, "a V&V failure must not leave the whole editor on its spinner");
            var failed = failing.LoadError;

            permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            failing.SetIteration(this.scenario.Data.Iteration);
            var loaded = failing.Model.Iteration;

            permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Throws(new InvalidOperationException("broken"));
            failing.SetIteration(this.scenario.Data.Iteration);
            var modelAfterFailedReload = failing.Model.Iteration;

            var other = new VandVRegisterScenario();
            var otherSetup = new EngineeringModelSetup { Iid = Guid.NewGuid() };
            otherSetup.RequiredRdl.Add(other.Data.Library);
            other.Data.EngineeringModel.EngineeringModelSetup = otherSetup;
            failing.SetIteration(other.Data.Iteration);
            var modelAfterFailedChange = failing.Model.Iteration;
            var rowsAfterFailedChange = failing.RegisterViewModel.Rows;
            var emptyStateAfterFailedChange = failing.ShowEmptyState;
            var needsSetupAfterFailedChange = failing.SetupViewModel.NeedsSetup;

            permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            failing.SetIteration(this.scenario.Data.Iteration);

            Assert.Multiple(() =>
            {
                Assert.That(initial, Is.Null);
                Assert.That(failed, Does.Contain("could not be read"));
                Assert.That(failing.LoadError, Is.Null, "the next reload that works clears it");
                Assert.That(loaded, Is.SameAs(this.scenario.Data.Iteration));
                Assert.That(modelAfterFailedReload, Is.SameAs(this.scenario.Data.Iteration), "a failed reload keeps what was read, under the banner");
                Assert.That(modelAfterFailedChange, Is.Null, "a failed change of iteration must not keep showing the old iteration");
                Assert.That(rowsAfterFailedChange, Is.Empty);
                Assert.That(needsSetupAfterFailedChange, Is.True);
                Assert.That(emptyStateAfterFailedChange, Is.False, "data that could not be read is not a model without V&V");
                Assert.That(failing.Model.Iteration, Is.SameAs(this.scenario.Data.Iteration));
            });
        }

        [Test]
        public void VerifyShowEmptyState()
        {
            var emptyLibrary = new ModelReferenceDataLibrary { Iid = Guid.NewGuid(), ShortName = "MRDL", Name = "Empty library" };
            this.AttachLibrary(emptyLibrary);
            this.viewModel.SetIteration(this.scenario.Data.Iteration);
            var withItems = this.viewModel.ShowEmptyState;

            var bare = new VandVRegisterScenario();
            var bareSetup = new EngineeringModelSetup { Iid = Guid.NewGuid(), RequiredRdl = { new ModelReferenceDataLibrary { Iid = Guid.NewGuid() } } };
            bare.Data.EngineeringModel.EngineeringModelSetup = bareSetup;
            bare.Data.Iteration.RequirementsSpecification.RemoveAll(x => x.ShortName is VandVConstants.VandVSpecificationShortName or "FAT");
            bare.Data.Iteration.Relationship.Clear();
            this.viewModel.SetIteration(bare.Data.Iteration);

            Assert.Multiple(() =>
            {
                Assert.That(withItems, Is.False, "V&V data made by the IME is shown even when some reference data is missing");
                Assert.That(this.viewModel.SetupViewModel.NeedsSetup, Is.True);
                Assert.That(this.viewModel.ShowEmptyState, Is.True, "no reference data and no V&V data");
            });
        }

        private void AttachLibrary(ModelReferenceDataLibrary library)
        {
            var setup = new EngineeringModelSetup { Iid = Guid.NewGuid() };
            setup.RequiredRdl.Add(library);
            this.scenario.Data.EngineeringModel.EngineeringModelSetup = setup;
        }
    }
}
