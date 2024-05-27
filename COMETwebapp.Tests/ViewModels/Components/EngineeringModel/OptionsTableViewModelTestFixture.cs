// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OptionsTableViewModelTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
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
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.ViewModels.Components.EngineeringModel
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4Web.Enumerations;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.ViewModels.Components.EngineeringModel.Options;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class OptionsTableViewModelTestFixture
    {
        private OptionsTableViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IPermissionService> permissionService;
        private Assembler assembler;
        private Mock<ILogger<OptionsTableViewModel>> loggerMock;
        private CDPMessageBus messageBus;
        private Iteration iteration;
        private Option option;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.permissionService = new Mock<IPermissionService>();
            this.messageBus = new CDPMessageBus();
            this.loggerMock = new Mock<ILogger<OptionsTableViewModel>>();

            this.option = new Option()
            {
                ShortName = "opt1",
                Name = "Option 1",
            };

            this.iteration = new Iteration() { Container = new EngineeringModel() };
            this.iteration.Option.Add(this.option);

            this.assembler = new Assembler(new Uri("http://localhost:5000/"), this.messageBus);
            var lazyOption = new Lazy<Thing>(this.option);
            this.assembler.Cache.TryAdd(new CacheKey(), lazyOption);

            this.permissionService.Setup(x => x.CanWrite(this.option.ClassKind, this.option.Container)).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            session.Setup(x => x.Assembler).Returns(this.assembler);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);

            this.viewModel = new OptionsTableViewModel(this.sessionService.Object, this.messageBus, this.loggerMock.Object);
            this.viewModel.SetCurrentIteration(this.iteration);
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
            this.viewModel.InitializeViewModel();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows.Count, Is.EqualTo(1));
                Assert.That(this.viewModel.Rows.Items.First().Thing, Is.EqualTo(this.option));
            });
        }

        [Test]
        public void VerifyOptionRowProperties()
        {
            this.viewModel.InitializeViewModel();
            var optionRow = this.viewModel.Rows.Items.First();

            Assert.Multiple(() =>
            {
                Assert.That(optionRow.Name, Is.EqualTo(this.option.Name));
                Assert.That(optionRow.ShortName, Is.EqualTo(this.option.ShortName));
                Assert.That(optionRow.IsDefault, Is.EqualTo(false));
                Assert.That(optionRow.Thing, Is.EqualTo(this.option));
                Assert.That(optionRow.IsAllowedToWrite, Is.EqualTo(true));
            });
        }

        [Test]
        public void VerifySessionRefreshAndEndUpdate()
        {
            this.viewModel.InitializeViewModel();
            this.viewModel.SetCurrentIteration(this.iteration);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.Rows.Items.First().IsDefault, Is.EqualTo(false));
            });

            this.iteration.DefaultOption = this.option;
            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);
            this.messageBus.SendMessage(new SessionEvent(null, SessionStatus.EndUpdate));

            Assert.That(this.viewModel.Rows.Items.First().IsDefault, Is.EqualTo(true));
        }

        [Test]
        public async Task VerifyOptionCreateOrEdit()
        {
            this.viewModel.InitializeViewModel();
            this.viewModel.SetCurrentIteration(this.iteration);
            this.viewModel.CurrentThing = this.option.Clone(true);

            Assert.That(this.viewModel.CurrentThing.Original, Is.Not.Null);
            await this.viewModel.CreateOrEditOption(true);
            this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<EngineeringModel>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Once);

            this.sessionService.Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>())).Throws(new Exception());
            this.viewModel.SelectedIsDefaultValue = true;

            await this.viewModel.CreateOrEditOption(false);
            this.loggerMock.Verify(LogLevel.Error, x => !string.IsNullOrWhiteSpace(x.ToString()), Times.Once());
        }
    }
}
