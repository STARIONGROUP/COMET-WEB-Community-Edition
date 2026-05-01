// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ModelEditorViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.ModelEditor
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.Cache;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.ModelEditor;

    using DynamicData;

    using FluentResults;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ModelEditorViewModelTestFixture
    {
        /// <summary>
        /// The view model under test.
        /// </summary>
        private ModelEditorViewModel viewModel;

        /// <summary>
        /// The mocked <see cref="ISessionService" />.
        /// </summary>
        private Mock<ISessionService> sessionService;

        /// <summary>
        /// The message bus used by the view model.
        /// </summary>
        private CDPMessageBus messageBus;

        /// <summary>
        /// The iteration providing the test fixture's element graph.
        /// </summary>
        private Iteration iteration;

        /// <summary>
        /// The iteration's top element — must not be deletable.
        /// </summary>
        private ElementDefinition topElement;

        /// <summary>
        /// A non-top <see cref="ElementDefinition" /> referenced by <see cref="elementUsage" />.
        /// </summary>
        private ElementDefinition referencedElementDefinition;

        /// <summary>
        /// An <see cref="ElementUsage" /> contained by <see cref="topElement" /> referencing
        /// <see cref="referencedElementDefinition" />.
        /// </summary>
        private ElementUsage elementUsage;

        /// <summary>
        /// Builds the iteration graph (TopElement, a referenced ElementDefinition, and a usage) and the
        /// view model with mocked dependencies.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.sessionService = new Mock<ISessionService>();
            var cacheService = new Mock<ICacheService>();

            var session = new Mock<ISession>();
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.sessionService.Setup(x => x.OpenIterations).Returns(new SourceList<Iteration>());

            var domain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            var rdl = new SiteReferenceDataLibrary { ShortName = "siteRdl", Name = "Site RDL" };
            var siteDirectory = new SiteDirectory { Domain = { domain }, SiteReferenceDataLibrary = { rdl } };
            var modelSetup = new EngineeringModelSetup { Name = "ModelName", ShortName = "ModelShortName", ActiveDomain = { domain }, RequiredRdl = { new ModelReferenceDataLibrary { RequiredRdl = rdl } } };
            siteDirectory.Model.Add(modelSetup);
            var iterationSetup = new IterationSetup { IterationNumber = 1 };
            modelSetup.IterationSetup.Add(iterationSetup);
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);
            session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);

            this.topElement = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Container",
                ShortName = "CONT",
                Owner = domain
            };

            this.referencedElementDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Box",
                ShortName = "BOX",
                Owner = domain
            };

            this.elementUsage = new ElementUsage
            {
                Iid = Guid.NewGuid(),
                Name = "Box1",
                ShortName = "BOX1",
                ElementDefinition = this.referencedElementDefinition,
                Owner = domain
            };

            this.topElement.ContainedElement.Add(this.elementUsage);

            this.iteration = new Iteration
            {
                Iid = Guid.NewGuid(),
                Element = { this.topElement, this.referencedElementDefinition },
                TopElement = this.topElement,
                IterationSetup = iterationSetup,
                Container = new EngineeringModel { EngineeringModelSetup = modelSetup }
            };

            var logger = new Mock<ILogger<ModelEditorViewModel>>();

            this.viewModel = new ModelEditorViewModel(this.sessionService.Object, this.messageBus, cacheService.Object, logger.Object)
            {
                CurrentThing = this.iteration
            };
        }

        /// <summary>
        /// Tears down the message bus and disposes of the view model.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifyOpenDeleteElementPopupForUsage()
        {
            this.viewModel.SelectElement(this.elementUsage);
            this.viewModel.OpenDeleteElementPopup();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnDeletionMode, Is.True);
                Assert.That(this.viewModel.DeleteElementPopupViewModel.IsVisible, Is.True);
                Assert.That(this.viewModel.DeleteElementPopupViewModel.ContentText, Does.Contain("Element Usage"));
                Assert.That(this.viewModel.DeleteElementPopupViewModel.ContentText, Does.Contain(this.elementUsage.Name));
                Assert.That(this.viewModel.DeleteElementPopupViewModel.ContentText, Does.Contain(this.topElement.Name),
                    "Popup must mention the parent ElementDefinition name.");
            });
        }

        [Test]
        public void VerifyOpenDeleteElementPopupForDefinition()
        {
            this.viewModel.SelectElement(this.referencedElementDefinition);
            this.viewModel.OpenDeleteElementPopup();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnDeletionMode, Is.True);
                Assert.That(this.viewModel.DeleteElementPopupViewModel.ContentText, Does.Contain("Element Definition"));
                Assert.That(this.viewModel.DeleteElementPopupViewModel.ContentText, Does.Contain(this.referencedElementDefinition.Name));
                Assert.That(this.viewModel.DeleteElementPopupViewModel.ContentText, Does.Not.Contain("referenced"),
                    "Server cascades cleanup, so the popup must not warn about referencing usages.");
            });
        }

        [Test]
        public void VerifyTopElementCannotBeDeleted()
        {
            this.viewModel.SelectElement(this.topElement);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsSelectedElementTopElement, Is.True);
            });

            this.viewModel.OpenDeleteElementPopup();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnDeletionMode, Is.False, "OpenDeleteElementPopup must be a no-op for the iteration's TopElement.");
                Assert.That(this.viewModel.DeleteElementPopupViewModel.IsVisible, Is.False);
            });
        }

        [Test]
        public async Task VerifyDeleteElementUsageCallsSessionServiceWithCorrectContainer()
        {
            this.sessionService
                .Setup(x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            this.viewModel.SelectElement(this.elementUsage);
            await this.viewModel.DeleteSelectedElementAsync();

            this.sessionService.Verify(
                x => x.DeleteThingsWithNotification(
                    It.Is<Thing>(t => t is ElementDefinition && t.Iid == this.topElement.Iid),
                    It.Is<IReadOnlyCollection<Thing>>(c => c.Count == 1 && c.Single() is ElementUsage && c.Single().Iid == this.elementUsage.Iid),
                    It.IsAny<NotificationDescription>()),
                Times.Once);
        }

        [Test]
        public async Task VerifyDeleteElementDefinitionCallsSessionServiceWithCorrectContainer()
        {
            this.sessionService
                .Setup(x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            this.viewModel.SelectElement(this.referencedElementDefinition);
            await this.viewModel.DeleteSelectedElementAsync();

            this.sessionService.Verify(
                x => x.DeleteThingsWithNotification(
                    It.Is<Thing>(t => t is Iteration && t.Iid == this.iteration.Iid),
                    It.Is<IReadOnlyCollection<Thing>>(c => c.Count == 1 && c.Single() is ElementDefinition && c.Single().Iid == this.referencedElementDefinition.Iid),
                    It.IsAny<NotificationDescription>()),
                Times.Once);
        }

        [Test]
        public async Task VerifyDeleteClearsSelectionOnSuccess()
        {
            this.sessionService
                .Setup(x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            this.viewModel.SelectElement(this.elementUsage);
            await this.viewModel.DeleteSelectedElementAsync();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedElement, Is.Null);
                Assert.That(this.viewModel.SelectedElementDefinition, Is.Null);
                Assert.That(this.viewModel.IsOnDeletionMode, Is.False);
                Assert.That(this.viewModel.DeleteElementPopupViewModel.IsVisible, Is.False);
            });
        }
    }
}
