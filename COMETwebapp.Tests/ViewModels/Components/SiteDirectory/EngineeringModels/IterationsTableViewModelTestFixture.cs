// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IterationsTableViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.SiteDirectory.EngineeringModels
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4Web.Enumerations;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class IterationsTableViewModelTestFixture
    {
        private IterationsTableViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IPermissionService> permissionService;
        private Mock<ILogger<IterationsTableViewModel>> loggerMock;
        private CDPMessageBus messageBus;
        private IterationSetup iteration;
        private EngineeringModelSetup model;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.permissionService = new Mock<IPermissionService>();
            this.messageBus = new CDPMessageBus();
            this.loggerMock = new Mock<ILogger<IterationsTableViewModel>>();

            this.iteration = new IterationSetup
            {
                IterationNumber = 1
            };

            this.model = new EngineeringModelSetup
            {
                Name = "model",
                ShortName = "model",
                StudyPhase = StudyPhaseKind.DESIGN_SESSION_PHASE
            };

            this.model.IterationSetup.Add(this.iteration);

            var siteDirectory = new SiteDirectory
            {
                ShortName = "siteDirectory"
            };

            siteDirectory.Model.Add(this.model);

            this.permissionService.Setup(x => x.CanWrite(this.iteration.ClassKind, this.iteration.Container)).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);

            this.viewModel = new IterationsTableViewModel(this.sessionService.Object, this.messageBus, this.loggerMock.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.ClearSubscriptions();
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifyInitializeViewModel()
        {
            this.viewModel.InitializeViewModel(this.model);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows.Count, Is.EqualTo(1));
                Assert.That(this.viewModel.Rows.Items[0].Thing, Is.EqualTo(this.iteration));
                Assert.That(this.viewModel.CanCreateIteration, Is.True);
                Assert.That(this.viewModel.SourceIterations.ToList(), Has.Count.EqualTo(1));
            });
        }

        [Test]
        public async Task VerifyIterationActions()
        {
            this.viewModel.InitializeViewModel(this.model);
            this.viewModel.CurrentThing = new IterationSetup();

            await this.viewModel.CreateOrEditIteration(false);
            this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<EngineeringModelSetup>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Once);

            await this.viewModel.CreateOrEditIteration(true);
            this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<EngineeringModelSetup>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Exactly(2));

            this.sessionService.Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>())).Throws(new Exception());
            await this.viewModel.CreateOrEditIteration(false);
            this.loggerMock.Verify(LogLevel.Error, x => !string.IsNullOrWhiteSpace(x.ToString()), Times.Once());
        }

        [Test]
        public void VerifyIterationRowProperties()
        {
            this.viewModel.InitializeViewModel(this.model);
            var row = this.viewModel.Rows.Items[0];

            Assert.Multiple(() =>
            {
                Assert.That(row.ContainerName, Is.EqualTo(this.model.ShortName));
                Assert.That(row.Name, Is.EqualTo("Iteration 1"));
                Assert.That(row.Thing, Is.EqualTo(this.iteration));
                Assert.That(row.IsAllowedToWrite, Is.True);
                Assert.That(row.Number, Is.EqualTo("1"));
            });
        }

        [Test]
        public async Task VerifyRowOperations()
        {
            this.viewModel.InitializeViewModel(this.model);
            var row = this.viewModel.Rows.Items[0];

            Assert.That(row, Is.Not.Null);

            this.viewModel.OnDeleteButtonClick(row);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnDeletionMode, Is.True);
                Assert.That(this.viewModel.CurrentThing, Is.EqualTo(row.Thing));
            });

            this.viewModel.OnCancelPopupButtonClick();
            Assert.That(this.viewModel.IsOnDeletionMode, Is.False);

            await this.viewModel.OnConfirmPopupButtonClick();
            this.sessionService.Verify(x => x.DeleteThingsWithNotification(It.IsAny<EngineeringModelSetup>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Once);
        }

        [Test]
        public void VerifySessionRefresh()
        {
            this.viewModel.InitializeViewModel(this.model);

            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);
            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));

            var iterationTest = new IterationSetup
            {
                Iid = Guid.NewGuid(),
                IterationNumber = 2,
                Container = this.model
            };

            this.messageBus.SendObjectChangeEvent(iterationTest, EventKind.Added);
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);

            this.messageBus.SendObjectChangeEvent(this.viewModel.Rows.Items[0].Thing, EventKind.Removed);
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);

            this.messageBus.SendObjectChangeEvent(this.viewModel.Rows.Items[0].Thing, EventKind.Updated);
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);

            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));
        }
    }
}
