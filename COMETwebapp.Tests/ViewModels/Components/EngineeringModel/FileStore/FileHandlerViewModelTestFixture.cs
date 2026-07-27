// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FileHandlerViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.EngineeringModel.FileStore
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileHandler;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileRevisionHandler;

    using FluentResults;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class FileHandlerViewModelTestFixture
    {
        private FileHandlerViewModel viewModel;
        private CDPMessageBus messageBus;
        private Mock<ISessionService> sessionService;
        private Mock<ILogger<FileHandlerViewModel>> logger;
        private Mock<IFileRevisionHandlerViewModel> fileRevisionHandlerViewModel;
        private CommonFileStore commonFileStore;
        private Iteration iteration;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.sessionService = new Mock<ISessionService>();
            this.logger = new Mock<ILogger<FileHandlerViewModel>>();
            this.fileRevisionHandlerViewModel = new Mock<IFileRevisionHandlerViewModel>();

            this.iteration = new Iteration();
            var person = new Person();

            var siteDirectory = new SiteDirectory
            {
                Domain =
                {
                    new DomainOfExpertise
                    {
                        ShortName = "doe",
                        Name = "Domain Of Expertise"
                    }
                }
            };

            var engineeringModelSetup = new EngineeringModelSetup
            {
                Participant = { new Participant { Person = person } }
            };

            var folder1 = new Folder
            {
                Iid = Guid.NewGuid(),
                Name = "folder 1"
            };

            var file1 = new File
            {
                Iid = Guid.NewGuid(),
                CurrentContainingFolder = folder1
            };

            var file2 = new File
            {
                Iid = Guid.NewGuid(),
                CurrentContainingFolder = folder1
            };

            file1.FileRevision.Add(new FileRevision());

            this.commonFileStore = new CommonFileStore
            {
                Name = "CFS",
                Folder = { folder1 },
                File = { file1, file2 },
                Owner = siteDirectory.Domain.First(),
                Container = new EngineeringModel { EngineeringModelSetup = engineeringModelSetup }
            };

            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);
            this.sessionService.Setup(x => x.Session.ActivePerson).Returns(person);
            this.sessionService.Setup(x => x.CreateOrUpdateThings(It.IsAny<Thing>(), It.IsAny<List<Thing>>(), It.IsAny<List<string>>())).ReturnsAsync(new Result());

            this.viewModel = new FileHandlerViewModel(this.sessionService.Object, this.messageBus, this.logger.Object, this.fileRevisionHandlerViewModel.Object);
        }

        [TearDown]
        public void TearDown()
        {
            this.viewModel.Dispose();
            this.messageBus.Dispose();
        }

        /// <summary>
        /// Verifies that disposing the view model disposes its <see cref="DomainOfExpertiseSelectorViewModel" /> — even
        /// though this view model is <c>ApplicationBaseViewModel</c>-derived, its constructor opts out of the tab-move
        /// dispose gate (it is a nested content view model, never a tab), so a plain <c>Dispose()</c> releases the
        /// selector's circuit-scoped message-bus subscription instead of leaking it (see GH824).
        /// </summary>
        [Test]
        public void VerifyDisposeUnsubscribesDomainSelectorFromMessageBus()
        {
            var domainA = new DomainOfExpertise { Iid = Guid.NewGuid() };
            var domainB = new DomainOfExpertise { Iid = Guid.NewGuid() };

            this.viewModel.DomainOfExpertiseSelectorViewModel.CurrentIteration = this.iteration;

            this.messageBus.SendMessage(new DomainChangedEvent(this.iteration, domainA));
            Assert.That(this.viewModel.DomainOfExpertiseSelectorViewModel.CurrentIterationDomain, Is.EqualTo(domainA), "The selector should react while subscribed.");

            this.viewModel.Dispose();
            this.messageBus.SendMessage(new DomainChangedEvent(this.iteration, domainB));

            Assert.That(this.viewModel.DomainOfExpertiseSelectorViewModel.CurrentIterationDomain, Is.EqualTo(domainA), "After disposal the selector must no longer react to bus events.");
        }

        [Test]
        public async Task VerifyCreateOrEditFolder()
        {
            this.viewModel.InitializeViewModel(this.commonFileStore, this.iteration);
            this.viewModel.CurrentThing = this.commonFileStore.File.First();

            var domain = new DomainOfExpertise();
            this.viewModel.DomainOfExpertiseSelectorViewModel.SelectedDomainOfExpertise = domain;
            Assert.That(this.viewModel.CurrentThing.Owner, Is.EqualTo(domain));

            await this.viewModel.CreateOrEditFile(false);
            this.sessionService.Verify(x => x.CreateOrUpdateThings(It.IsAny<FileStore>(), It.Is<IReadOnlyCollection<Thing>>(c => !c.OfType<FileStore>().Any()), It.IsAny<IReadOnlyCollection<string>>()), Times.Once);

            this.viewModel.SelectedFileRevisions = [new FileRevision { LocalPath = "/localpath" }];
            this.viewModel.IsLocked = true;

            var failingResult = new Result();
            failingResult.Reasons.Add(new ExceptionalError(new Exception("Invalid")));
            this.sessionService.Setup(x => x.CreateOrUpdateThings(It.IsAny<Thing>(), It.IsAny<List<Thing>>(), It.IsAny<List<string>>())).ReturnsAsync(failingResult);

            await this.viewModel.CreateOrEditFile(true);

            Assert.Multiple(() =>
            {
                this.sessionService.Verify(x => x.CreateOrUpdateThings(
                        It.IsAny<FileStore>(),
                        It.Is<IReadOnlyCollection<Thing>>(c => c.OfType<FileStore>().Any()),
                        It.Is<IReadOnlyCollection<string>>(c => c.Contains("/localpath")))
                    , Times.Once);

                Assert.That(this.viewModel.CurrentThing.LockedBy, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyInitializeViewModel()
        {
            this.viewModel.InitializeViewModel(this.commonFileStore, this.iteration);
            Assert.That(this.viewModel.DomainOfExpertiseSelectorViewModel.AvailableDomainsOfExpertise.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task VerifyMoveAndDeleteFile()
        {
            this.viewModel.InitializeViewModel(this.commonFileStore, this.iteration);
            this.viewModel.CurrentThing = this.commonFileStore.File[0];

            await this.viewModel.MoveFile(this.commonFileStore.File[0], this.commonFileStore.Folder[0]);
            this.sessionService.Verify(x => x.CreateOrUpdateThings(It.IsAny<FileStore>(), It.IsAny<IReadOnlyCollection<Thing>>()), Times.Once);

            await this.viewModel.DeleteFile();
            this.sessionService.Verify(x => x.DeleteThings(It.IsAny<FileStore>(), It.IsAny<IReadOnlyCollection<Thing>>()), Times.Once);
        }
    }
}
