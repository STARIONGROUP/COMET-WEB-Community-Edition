// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVSetupViewModelTestFixture.cs" company="Starion Group S.A.">
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

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Tests.Utilities.VandV;
    using COMETwebapp.Utilities.VandV;
    using COMETwebapp.ViewModels.Components.VandV;

    using FluentResults;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class VandVSetupViewModelTestFixture
    {
        private static readonly string[] MovedGates = ["PDR", "SRR", "CDR"];

        private VandVSetupViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IPermissionService> permissionService;
        private Mock<ISession> session;
        private ModelReferenceDataLibrary modelLibrary;
        private Iteration iteration;
        private Thing writtenTopContainer;
        private List<Thing> writtenThings;

        [SetUp]
        public void SetUp()
        {
            var siteLibrary = new SiteReferenceDataLibrary { Iid = Guid.NewGuid(), ShortName = "SRDL", Name = "Site library" };
            siteLibrary.DefinedCategory.Add(new Category { Iid = Guid.NewGuid(), ShortName = "REQ", Name = "Requirement", PermissibleClass = { ClassKind.Requirement } });
            this.modelLibrary = new ModelReferenceDataLibrary { Iid = Guid.NewGuid(), ShortName = "MRDL", Name = "Model library", RequiredRdl = siteLibrary };

            var setup = new EngineeringModelSetup { Iid = Guid.NewGuid() };
            setup.RequiredRdl.Add(this.modelLibrary);
            var engineeringModel = new EngineeringModel { Iid = Guid.NewGuid(), EngineeringModelSetup = setup };
            this.iteration = new Iteration { Iid = Guid.NewGuid() };
            engineeringModel.Iteration.Add(this.iteration);

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.Refresh()).Returns(Task.CompletedTask);
            this.sessionService = new Mock<ISessionService>();
            this.sessionService.Setup(x => x.Session).Returns(this.session.Object);

            this.writtenThings = [];

            this.sessionService.Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback<Thing, IReadOnlyCollection<Thing>, NotificationDescription>((top, things, _) =>
                {
                    this.writtenTopContainer = top;
                    this.writtenThings = things.ToList();
                })
                .ReturnsAsync(Result.Ok());

            this.viewModel = new VandVSetupViewModel(this.sessionService.Object, new Mock<ILogger>().Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifySetIteration()
        {
            var initial = this.viewModel.NeedsSetup;
            this.viewModel.SetIteration(new Iteration { Iid = Guid.NewGuid() });
            var withoutModel = (this.viewModel.TargetLibrary, this.viewModel.NeedsSetup);

            this.viewModel.SetIteration(this.iteration);

            Assert.Multiple(() =>
            {
                Assert.That(initial, Is.False);
                Assert.That(withoutModel.TargetLibrary, Is.Null);
                Assert.That(withoutModel.NeedsSetup, Is.False, "nothing to seed into");
                Assert.That(this.viewModel.TargetLibrary, Is.SameAs(this.modelLibrary));
                Assert.That(this.viewModel.NeedsSetup, Is.True);
                Assert.That(this.viewModel.CanSeed, Is.True);
                Assert.That(this.viewModel.NeedsStageGates, Is.True);
                Assert.That(this.viewModel.MissingItems, Is.Not.Empty);
            });

            this.permissionService.Setup(x => x.CanWrite(ClassKind.ParameterizedCategoryRule, this.modelLibrary)).Returns(false);
            this.viewModel.SetIteration(this.iteration);

            Assert.That(this.viewModel.CanSeed, Is.False, "one missing class the user may not create is enough to refuse");
        }

        [Test]
        public async Task VerifyOpen()
        {
            this.viewModel.SetIteration(this.iteration);
            this.viewModel.Open();
            this.viewModel.SetStageGate(0, "Changed");
            this.permissionService.Setup(x => x.CanWrite(ClassKind.Category, this.modelLibrary)).Returns(false);
            await this.viewModel.SeedAsync();
            var errorAfterFailure = this.viewModel.LastError;

            this.viewModel.IsVisible = false;
            this.viewModel.Open();

            Assert.Multiple(() =>
            {
                Assert.That(errorAfterFailure, Is.Not.Null);
                Assert.That(this.viewModel.LastError, Is.Null, "reopening forgets the previous attempt's error");
                Assert.That(this.viewModel.IsVisible, Is.True);
                Assert.That(this.viewModel.StageGates, Is.EqualTo(VandVConstants.DefaultStageGates), "every opening starts from the example gates");
                Assert.That(this.viewModel.StageGateError, Is.Null);
            });
        }

        [Test]
        public void VerifyAddStageGate()
        {
            this.viewModel.AddStageGate();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.StageGates, Has.Count.EqualTo(VandVConstants.DefaultStageGates.Count + 1));
                Assert.That(this.viewModel.StageGates[^1], Is.Empty, "a new gate starts empty");
                Assert.That(this.viewModel.StageGateError, Is.Null, "a blank row is ignored");
            });
        }

        [Test]
        public void VerifyRemoveStageGate()
        {
            this.viewModel.RemoveStageGate(99);
            this.viewModel.RemoveStageGate(-1);
            var unchanged = this.viewModel.StageGates.ToList();
            this.viewModel.RemoveStageGate(0);

            Assert.Multiple(() =>
            {
                Assert.That(unchanged, Is.EqualTo(VandVConstants.DefaultStageGates), "out of range does nothing");
                Assert.That(this.viewModel.StageGates, Is.EqualTo(VandVConstants.DefaultStageGates.Skip(1)));
            });
        }

        [Test]
        public void VerifyMoveStageGate()
        {
            this.viewModel.MoveStageGate(0, -1);
            this.viewModel.MoveStageGate(VandVConstants.DefaultStageGates.Count - 1, 1);
            this.viewModel.MoveStageGate(99, -1);
            var unchanged = this.viewModel.StageGates.ToList();
            this.viewModel.MoveStageGate(0, 1);

            Assert.Multiple(() =>
            {
                Assert.That(unchanged, Is.EqualTo(VandVConstants.DefaultStageGates), "nothing moves past either end");
                Assert.That(this.viewModel.StageGates.Take(3), Is.EqualTo(MovedGates));
            });
        }

        [Test]
        public void VerifySetStageGate()
        {
            this.viewModel.SetStageGate(-1, "ignored");
            this.viewModel.SetStageGate(99, "ignored");
            this.viewModel.SetStageGate(0, "MDR");

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.StageGates, Does.Not.Contain("ignored"));
                Assert.That(this.viewModel.StageGates[0], Is.EqualTo("MDR"));
                Assert.That(this.viewModel.StageGates.Skip(1), Is.EqualTo(VandVConstants.DefaultStageGates.Skip(1)));
            });
        }

        [Test]
        public async Task VerifySeedAsync()
        {
            this.viewModel.SetIteration(this.iteration);
            this.viewModel.Open();

            foreach (var _ in VandVConstants.DefaultStageGates)
            {
                this.viewModel.RemoveStageGate(0);
            }

            var withoutGates = await this.viewModel.SeedAsync();

            this.viewModel.AddStageGate();
            this.viewModel.SetStageGate(0, "PDR");
            this.viewModel.AddStageGate();
            this.viewModel.SetStageGate(1, "CDR");
            var seeded = await this.viewModel.SeedAsync();

            var libraryClone = (ModelReferenceDataLibrary)this.writtenTopContainer;
            var stage = this.writtenThings.OfType<EnumerationParameterType>().Single(x => x.ShortName == VandVConstants.Stage);

            Assert.Multiple(() =>
            {
                Assert.That(withoutGates.IsFailed, Is.True);
                Assert.That(withoutGates.Errors[0].Message, Is.EqualTo("Define at least one stage gate."));
                Assert.That(this.viewModel.LastError, Is.Null, "a success clears the previous error");
                this.session.Verify(x => x.Refresh(), Times.Exactly(2), "every attempt first refreshes the session to see a set-up made meanwhile");
                Assert.That(seeded.IsSuccess, Is.True);
                this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Once);
                Assert.That(libraryClone.Iid, Is.EqualTo(this.modelLibrary.Iid), "one transaction with the model library clone as top container");
                Assert.That(libraryClone, Is.Not.SameAs(this.modelLibrary));
                Assert.That(this.writtenThings[0], Is.SameAs(libraryClone));
                Assert.That(this.writtenThings, Has.Count.GreaterThan(VandVManifest.ParameterTypes.Count + VandVManifest.Categories.Count));
                Assert.That(stage.ValueDefinition.Select(x => x.Name), Is.EqualTo(new[] { "PDR", "CDR" }));
                Assert.That(this.viewModel.IsVisible, Is.False, "closed on success");
                Assert.That(this.viewModel.IsBusy, Is.False);
            });
        }

        [Test]
        public async Task VerifySeedAsyncRefusals()
        {
            this.viewModel.SetIteration(this.iteration);
            this.permissionService.Setup(x => x.CanWrite(ClassKind.Category, this.modelLibrary)).Returns(false);
            var notAllowed = await this.viewModel.SeedAsync();

            var complete = new VandVTestData();
            complete.Library.Rule.AddRange(VandVManifest.ParameterizedCategoryRules.Select(x => new ParameterizedCategoryRule { Iid = Guid.NewGuid(), ShortName = x.ShortName }));
            complete.Library.Rule.AddRange(VandVManifest.BinaryRelationshipRules.Select(x => new BinaryRelationshipRule { Iid = Guid.NewGuid(), ShortName = x.ShortName }));
            var completeSetup = new EngineeringModelSetup { Iid = Guid.NewGuid() };
            completeSetup.RequiredRdl.Add(complete.Library);
            complete.EngineeringModel.EngineeringModelSetup = completeSetup;
            this.viewModel.SetIteration(complete.Iteration);
            this.viewModel.IsVisible = true;
            var nothingToDo = await this.viewModel.SeedAsync();

            Assert.Multiple(() =>
            {
                Assert.That(notAllowed.IsFailed, Is.True);
                Assert.That(this.viewModel.NeedsSetup, Is.False, "a model set up by the IME needs nothing");
                Assert.That(nothingToDo.IsSuccess, Is.True);
                Assert.That(this.viewModel.IsVisible, Is.False);
                this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Never);
            });
        }

        [Test]
        public async Task VerifySeedAsyncFailures()
        {
            this.viewModel.SetIteration(this.iteration);
            this.viewModel.Open();

            this.sessionService.Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Fail("server says no"));

            var refused = await this.viewModel.SeedAsync();
            var stillOpen = this.viewModel.IsVisible;

            this.sessionService.Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var thrown = await this.viewModel.SeedAsync();

            Assert.Multiple(() =>
            {
                Assert.That(refused.IsFailed, Is.True);
                Assert.That(stillOpen, Is.True, "the dialog stays open so the user sees the failure");
                Assert.That(thrown.Errors[0].Message, Is.EqualTo("boom"));
                Assert.That(this.viewModel.LastError, Is.EqualTo("boom"));
                Assert.That(this.viewModel.IsBusy, Is.False);
            });
        }

        [Test]
        public async Task VerifySeedAsyncRefusesASecondCallWhileBusy()
        {
            var pendingWrite = new TaskCompletionSource<Result>();

            this.sessionService.Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .Returns(pendingWrite.Task);

            this.viewModel.SetIteration(this.iteration);
            this.viewModel.Open();

            var first = this.viewModel.SeedAsync();
            var busyDuringWrite = this.viewModel.IsBusy;

            // bounded, so a missing guard fails the test instead of hanging it on the held write
            var second = this.viewModel.SeedAsync();
            var secondReturnedAtOnce = await Task.WhenAny(second, Task.Delay(TimeSpan.FromSeconds(2))) == second;

            pendingWrite.SetResult(Result.Ok());
            var firstResult = await first;
            var secondResult = await second;

            Assert.Multiple(() =>
            {
                Assert.That(busyDuringWrite, Is.True);
                Assert.That(secondReturnedAtOnce, Is.True, "the second call is refused, not queued behind the first");
                Assert.That(secondResult.IsFailed, Is.True, "a double click must not seed a second, duplicate set");
                Assert.That(firstResult.IsSuccess, Is.True);
                this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Once);
                Assert.That(this.viewModel.IsBusy, Is.False);
            });
        }
    }
}
