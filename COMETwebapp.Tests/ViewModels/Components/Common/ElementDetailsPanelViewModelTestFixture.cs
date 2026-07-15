// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDetailsPanelViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.Common
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.Common;

    using DynamicData;

    using FluentResults;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class ElementDetailsPanelViewModelTestFixture
    {
        /// <summary>
        /// The view model under test.
        /// </summary>
        private ElementDetailsPanelViewModel viewModel;

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
        /// A <see cref="Parameter" /> contained by <see cref="referencedElementDefinition" /> used to
        /// exercise the parameter delete flow.
        /// </summary>
        private Parameter parameter;

        /// <summary>
        /// The currently logged-in <see cref="DomainOfExpertise" /> against which the view model is
        /// evaluated. Returned by the mocked <see cref="ISessionService.GetDomainOfExpertise" />.
        /// </summary>
        private DomainOfExpertise currentDomain;

        /// <summary>
        /// A foreign <see cref="DomainOfExpertise" /> — the owner of <see cref="foreignParameter" /> — used
        /// to exercise the subscribe / unsubscribe flows from the perspective of
        /// <see cref="currentDomain" />.
        /// </summary>
        private DomainOfExpertise foreignDomain;

        /// <summary>
        /// A <see cref="Parameter" /> owned by <see cref="foreignDomain" /> used to exercise the
        /// subscription create flow.
        /// </summary>
        private Parameter foreignParameter;

        /// <summary>
        /// Builds the iteration graph (TopElement, a referenced ElementDefinition, and a usage) and the
        /// view model with mocked dependencies.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.sessionService = new Mock<ISessionService>();

            var session = new Mock<ISession>();
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.sessionService.Setup(x => x.OpenIterations).Returns(new SourceList<Iteration>());

            var domain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            this.currentDomain = domain;
            this.foreignDomain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "PWR", Name = "Power" };
            this.sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(domain);
            var rdl = new SiteReferenceDataLibrary { ShortName = "siteRdl", Name = "Site RDL" };
            var siteDirectory = new SiteDirectory { Domain = { domain, this.foreignDomain }, SiteReferenceDataLibrary = { rdl } };
            var modelSetup = new EngineeringModelSetup { Name = "ModelName", ShortName = "ModelShortName", ActiveDomain = { domain, this.foreignDomain }, RequiredRdl = { new ModelReferenceDataLibrary { RequiredRdl = rdl } } };
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

            var parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "Mass", ShortName = "m" };
            rdl.ParameterType.Add(parameterType);

            this.parameter = new Parameter
            {
                Iid = Guid.NewGuid(),
                Owner = domain,
                ParameterType = parameterType
            };

            this.referencedElementDefinition.Parameter.Add(this.parameter);

            this.foreignParameter = new Parameter
            {
                Iid = Guid.NewGuid(),
                Owner = this.foreignDomain,
                ParameterType = parameterType
            };

            this.foreignParameter.ValueSet.Add(new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                Manual = new ValueArray<string>(["1"]),
                Computed = new ValueArray<string>(["1"]),
                Reference = new ValueArray<string>(["-"]),
                Formula = new ValueArray<string>(["-"]),
                Published = new ValueArray<string>(["1"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            });

            this.referencedElementDefinition.Parameter.Add(this.foreignParameter);

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

            var logger = new Mock<ILogger<ElementDetailsPanelViewModel>>();

            this.viewModel = new ElementDetailsPanelViewModel(this.sessionService.Object, this.messageBus, logger.Object);
            this.viewModel.CurrentIteration = this.iteration;
            this.viewModel.CurrentDomain = this.currentDomain;
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
        public void VerifyOpenEditParameterPopup()
        {
            this.viewModel.SelectElement(this.referencedElementDefinition);
            this.viewModel.OpenEditParameterPopup(this.parameter);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnEditParameterMode, Is.True);
                Assert.That(this.viewModel.EditParameterViewModel.Parameter, Is.Not.Null);
                Assert.That(this.viewModel.EditParameterViewModel.Parameter.Iid, Is.EqualTo(this.parameter.Iid));
                Assert.That(this.viewModel.EditParameterViewModel.Parameter, Is.Not.SameAs(this.parameter));
            });

            this.viewModel.IsOnEditParameterMode = false;
            this.viewModel.OpenEditParameterPopup(null);

            Assert.That(this.viewModel.IsOnEditParameterMode, Is.False, "A null parameter must not open the popup.");
        }

        [Test]
        public void VerifyEditRoutesToSubscriptionWhenSubscribed()
        {
            var subscription = new ParameterSubscription { Iid = Guid.NewGuid(), Owner = this.currentDomain };

            subscription.ValueSet.Add(new ParameterSubscriptionValueSet
            {
                Iid = Guid.NewGuid(),
                SubscribedValueSet = this.foreignParameter.ValueSet[0],
                Manual = new ValueArray<string>(["-"]),
                ValueSwitch = ParameterSwitchKind.COMPUTED
            });

            this.foreignParameter.ParameterSubscription.Add(subscription);
            this.viewModel.SelectElement(this.referencedElementDefinition);

            // Parameter the current domain subscribes to (but does not own) -> subscription dialog.
            this.viewModel.OpenEditParameterPopup(this.foreignParameter);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnEditSubscriptionMode, Is.True);
                Assert.That(this.viewModel.IsOnEditParameterMode, Is.False);
                Assert.That(this.viewModel.EditParameterSubscriptionViewModel.Subscription, Is.Not.Null);
            });

            this.viewModel.IsOnEditSubscriptionMode = false;

            // Parameter owned by the current domain -> parameter dialog.
            this.viewModel.OpenEditParameterPopup(this.parameter);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnEditParameterMode, Is.True);
                Assert.That(this.viewModel.IsOnEditSubscriptionMode, Is.False);
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

        [Test]
        public void VerifyOpenDeleteParameterPopupComposesContent()
        {
            this.viewModel.SelectElement(this.referencedElementDefinition);
            this.viewModel.OpenDeleteParameterPopup(this.parameter);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.DeleteParameterPopupViewModel.IsVisible, Is.True);
                Assert.That(this.viewModel.DeleteParameterPopupViewModel.ContentText, Does.Contain("Parameter"));
                Assert.That(this.viewModel.DeleteParameterPopupViewModel.ContentText, Does.Contain(this.parameter.ParameterType.Name));
                Assert.That(this.viewModel.DeleteParameterPopupViewModel.ContentText, Does.Contain(this.referencedElementDefinition.Name),
                    "Popup must mention the containing ElementDefinition name.");
            });
        }

        [Test]
        public void VerifyOpenDeleteParameterPopupNullIsNoOp()
        {
            this.viewModel.OpenDeleteParameterPopup(null);

            Assert.That(this.viewModel.DeleteParameterPopupViewModel.IsVisible, Is.False);
        }

        [Test]
        public async Task VerifyDeleteParameterCallsSessionServiceWithCorrectContainer()
        {
            this.sessionService
                .Setup(x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            this.viewModel.SelectElement(this.referencedElementDefinition);
            this.viewModel.OpenDeleteParameterPopup(this.parameter);
            await this.viewModel.DeleteSelectedParameterAsync();

            this.sessionService.Verify(
                x => x.DeleteThingsWithNotification(
                    It.Is<Thing>(t => t is ElementDefinition && t.Iid == this.referencedElementDefinition.Iid),
                    It.Is<IReadOnlyCollection<Thing>>(c => c.Count == 1 && c.Single() is Parameter && c.Single().Iid == this.parameter.Iid),
                    It.IsAny<NotificationDescription>()),
                Times.Once);
        }

        [Test]
        public async Task VerifyDeleteParameterClosesPopupOnSuccess()
        {
            this.sessionService
                .Setup(x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            this.viewModel.SelectElement(this.referencedElementDefinition);
            this.viewModel.OpenDeleteParameterPopup(this.parameter);
            await this.viewModel.DeleteSelectedParameterAsync();

            Assert.That(this.viewModel.DeleteParameterPopupViewModel.IsVisible, Is.False);
        }

        [Test]
        public async Task VerifyDeleteParameterWithoutTargetIsNoOp()
        {
            await this.viewModel.DeleteSelectedParameterAsync();

            this.sessionService.Verify(
                x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()),
                Times.Never);
        }

        [Test]
        public void VerifyOpenEditElementPopupForDefinitionInitialisesEditViewModel()
        {
            this.viewModel.SelectElement(this.referencedElementDefinition);
            this.viewModel.OpenEditElementPopup();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnEditMode, Is.True);
                Assert.That(this.viewModel.EditElementDefinitionViewModel.ElementDefinition, Is.Not.Null);
                Assert.That(this.viewModel.EditElementDefinitionViewModel.ElementDefinition, Is.Not.SameAs(this.referencedElementDefinition),
                    "OpenEditElementPopup must hand the form a clone, not the cached domain instance.");
                Assert.That(this.viewModel.EditElementDefinitionViewModel.ElementDefinition.Iid, Is.EqualTo(this.referencedElementDefinition.Iid));
                Assert.That(this.viewModel.EditElementDefinitionViewModel.IsTopElement, Is.False);
            });
        }

        [Test]
        public void VerifyOpenEditElementPopupForTopElementMarksTopElementFlag()
        {
            this.viewModel.SelectElement(this.topElement);
            this.viewModel.OpenEditElementPopup();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnEditMode, Is.True);
                Assert.That(this.viewModel.EditElementDefinitionViewModel.IsTopElement, Is.True);
            });
        }

        [Test]
        public void VerifyOpenEditElementPopupForUsageInitialisesUsageViewModel()
        {
            this.viewModel.SelectElement(this.elementUsage);
            this.viewModel.OpenEditElementPopup();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnEditMode, Is.True);
                Assert.That(this.viewModel.EditElementUsageViewModel.ElementUsage, Is.Not.Null);
                Assert.That(this.viewModel.EditElementUsageViewModel.ElementUsage, Is.Not.SameAs(this.elementUsage),
                    "OpenEditElementPopup must hand the form a clone, not the cached domain instance.");
                Assert.That(this.viewModel.EditElementUsageViewModel.ElementUsage.Iid, Is.EqualTo(this.elementUsage.Iid));
            });
        }

        [Test]
        public void VerifyOpenEditElementPopupWithNoSelectionIsNoOp()
        {
            this.viewModel.OpenEditElementPopup();

            Assert.That(this.viewModel.IsOnEditMode, Is.False);
        }

        [Test]
        public async Task VerifyEditElementDefinitionPromotesToTopElementWhenRequested()
        {
            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            this.viewModel.SelectElement(this.referencedElementDefinition);
            this.viewModel.OpenEditElementPopup();
            this.viewModel.EditElementDefinitionViewModel.IsTopElement = true;

            await this.viewModel.EditElementDefinitionAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(
                    It.Is<Thing>(t => t is Iteration && ((Iteration)t).TopElement != null && ((Iteration)t).TopElement.Iid == this.referencedElementDefinition.Iid),
                    It.Is<IReadOnlyCollection<Thing>>(c => c.OfType<Iteration>().Any() && c.OfType<ElementDefinition>().Any(d => d.Iid == this.referencedElementDefinition.Iid)),
                    It.IsAny<NotificationDescription>()),
                Times.Once);

            Assert.That(this.viewModel.IsOnEditMode, Is.False);
        }

        [Test]
        public async Task VerifyEditElementDefinitionAppliesSelectedCategories()
        {
            var newCategory = new Category { Iid = Guid.NewGuid(), Name = "Sensor", ShortName = "SEN", PermissibleClass = { ClassKind.ElementDefinition } };

            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            this.viewModel.SelectElement(this.referencedElementDefinition);
            this.viewModel.OpenEditElementPopup();
            this.viewModel.EditElementDefinitionViewModel.SelectedCategories = [newCategory];

            await this.viewModel.EditElementDefinitionAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(
                    It.IsAny<Thing>(),
                    It.Is<IReadOnlyCollection<Thing>>(c => c.OfType<ElementDefinition>().Any(d => d.Iid == this.referencedElementDefinition.Iid && d.Category.Contains(newCategory))),
                    It.IsAny<NotificationDescription>()),
                Times.Once);
        }

        [Test]
        public async Task VerifyEditElementUsageCallsSessionServiceWithCorrectContainer()
        {
            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            this.viewModel.SelectElement(this.elementUsage);
            this.viewModel.OpenEditElementPopup();

            await this.viewModel.EditElementUsageAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(
                    It.Is<Thing>(t => t is ElementDefinition && t.Iid == this.topElement.Iid),
                    It.Is<IReadOnlyCollection<Thing>>(c => c.Count == 1 && c.Single() is ElementUsage && c.Single().Iid == this.elementUsage.Iid),
                    It.IsAny<NotificationDescription>()),
                Times.Once);

            Assert.That(this.viewModel.IsOnEditMode, Is.False);
        }

        [Test]
        public async Task VerifyEditElementDefinitionWithoutTargetIsNoOp()
        {
            await this.viewModel.EditElementDefinitionAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()),
                Times.Never);

            Assert.That(this.viewModel.IsOnEditMode, Is.False);
        }

        [Test]
        public async Task VerifyEditElementUsageWithoutTargetIsNoOp()
        {
            await this.viewModel.EditElementUsageAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()),
                Times.Never);

            Assert.That(this.viewModel.IsOnEditMode, Is.False);
        }

        [Test]
        public void VerifyOpenEditElementPopupHoldsOriginalIterationForSelectorResolution()
        {
            this.viewModel.SelectElement(this.referencedElementDefinition);
            this.viewModel.OpenEditElementPopup();

            Assert.That(this.viewModel.EditElementDefinitionViewModel.Iteration, Is.SameAs(this.iteration),
                "OpenEditElementPopup must hand the edit VM the original iteration. Cloning here breaks DomainOfExpertiseSelectorViewModel which resolves the iteration through the open session.");
        }

        [Test]
        public async Task VerifyEditElementDefinitionClonesIterationAtSubmit()
        {
            Iteration capturedTopContainer = null;

            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback<Thing, IReadOnlyCollection<Thing>, NotificationDescription>((top, _, _) => capturedTopContainer = top as Iteration)
                .ReturnsAsync(Result.Ok());

            this.viewModel.SelectElement(this.referencedElementDefinition);
            this.viewModel.OpenEditElementPopup();

            await this.viewModel.EditElementDefinitionAsync();

            Assert.Multiple(() =>
            {
                Assert.That(capturedTopContainer, Is.Not.Null);
                Assert.That(capturedTopContainer, Is.Not.SameAs(this.iteration),
                    "EditElementDefinitionAsync must clone the iteration at submit time, not commit the cached instance.");
                Assert.That(capturedTopContainer.Iid, Is.EqualTo(this.iteration.Iid));
            });
        }

        [Test]
        public async Task VerifyEditElementDefinitionExceptionIsCaughtAndPopupClosed()
        {
            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            this.viewModel.SelectElement(this.referencedElementDefinition);
            this.viewModel.OpenEditElementPopup();

            await this.viewModel.EditElementDefinitionAsync();

            Assert.That(this.viewModel.IsOnEditMode, Is.False);
        }

        [Test]
        public void VerifyOpenCreateSubscriptionPopupComposesContent()
        {
            this.viewModel.SelectElement(this.referencedElementDefinition);
            this.viewModel.OpenCreateSubscriptionPopup(this.foreignParameter);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CreateSubscriptionPopupViewModel.IsVisible, Is.True);
                Assert.That(this.viewModel.CreateSubscriptionPopupViewModel.ContentText, Does.Contain(this.foreignParameter.ParameterType.Name));
                Assert.That(this.viewModel.CreateSubscriptionPopupViewModel.ContentText, Does.Contain(this.referencedElementDefinition.Name));
                Assert.That(this.viewModel.CreateSubscriptionPopupViewModel.ContentText, Does.Contain(this.currentDomain.ShortName));
            });
        }

        [Test]
        public void VerifyOpenCreateSubscriptionPopupNullParameterIsNoOp()
        {
            this.viewModel.OpenCreateSubscriptionPopup(null);

            Assert.That(this.viewModel.CreateSubscriptionPopupViewModel.IsVisible, Is.False);
        }

        [Test]
        public void VerifyOpenCreateSubscriptionPopupForOwnedParameterIsNoOp()
        {
            this.viewModel.OpenCreateSubscriptionPopup(this.parameter);

            Assert.That(this.viewModel.CreateSubscriptionPopupViewModel.IsVisible, Is.False,
                "Subscribing to a Parameter the current domain already owns is meaningless and must not open the popup.");
        }

        [Test]
        public async Task VerifyCreateSubscriptionCallsSessionServiceWithExpectedPayload()
        {
            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            this.viewModel.SelectElement(this.referencedElementDefinition);
            this.viewModel.OpenCreateSubscriptionPopup(this.foreignParameter);
            await this.viewModel.CreateSubscriptionAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(
                    It.Is<Thing>(t => t is Parameter && t.Iid == this.foreignParameter.Iid),
                    It.Is<IReadOnlyCollection<Thing>>(c =>
                        c.OfType<Parameter>().Any(p => p.Iid == this.foreignParameter.Iid)
                        && c.OfType<ParameterSubscription>().Any(s => s.Owner != null && s.Owner.Iid == this.currentDomain.Iid)
                        && c.OfType<ParameterSubscriptionValueSet>().Count() == this.foreignParameter.ValueSet.Count),
                    It.IsAny<NotificationDescription>()),
                Times.Once);

            Assert.That(this.viewModel.CreateSubscriptionPopupViewModel.IsVisible, Is.False);
        }

        [Test]
        public async Task VerifyCreateSubscriptionWithoutTargetIsNoOp()
        {
            await this.viewModel.CreateSubscriptionAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()),
                Times.Never);
        }

        [Test]
        public void VerifyOpenDeleteSubscriptionPopupComposesContent()
        {
            var subscription = new ParameterSubscription
            {
                Iid = Guid.NewGuid(),
                Owner = this.currentDomain
            };

            this.foreignParameter.ParameterSubscription.Add(subscription);

            this.viewModel.OpenDeleteSubscriptionPopup(subscription);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.DeleteSubscriptionPopupViewModel.IsVisible, Is.True);
                Assert.That(this.viewModel.DeleteSubscriptionPopupViewModel.ContentText, Does.Contain(this.foreignParameter.ParameterType.Name));
                Assert.That(this.viewModel.DeleteSubscriptionPopupViewModel.ContentText, Does.Contain("not be removed"),
                    "Popup must make it explicit that the parent Parameter is preserved.");
            });
        }

        [Test]
        public void VerifyOpenDeleteSubscriptionPopupNullIsNoOp()
        {
            this.viewModel.OpenDeleteSubscriptionPopup(null);

            Assert.That(this.viewModel.DeleteSubscriptionPopupViewModel.IsVisible, Is.False);
        }

        [Test]
        public void VerifyOpenDeleteSubscriptionPopupOrphanIsNoOp()
        {
            var orphanSubscription = new ParameterSubscription
            {
                Iid = Guid.NewGuid(),
                Owner = this.currentDomain
            };

            this.viewModel.OpenDeleteSubscriptionPopup(orphanSubscription);

            Assert.That(this.viewModel.DeleteSubscriptionPopupViewModel.IsVisible, Is.False,
                "A subscription whose Container is not a Parameter cannot be safely deleted from this flow.");
        }

        [Test]
        public async Task VerifyDeleteSubscriptionDoesNotIncludeParentParameterInDeleteList()
        {
            var subscription = new ParameterSubscription
            {
                Iid = Guid.NewGuid(),
                Owner = this.currentDomain
            };

            this.foreignParameter.ParameterSubscription.Add(subscription);

            this.sessionService
                .Setup(x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            this.viewModel.OpenDeleteSubscriptionPopup(subscription);
            await this.viewModel.DeleteSelectedSubscriptionAsync();

            this.sessionService.Verify(
                x => x.DeleteThingsWithNotification(
                    It.Is<Thing>(t => t is Parameter && t.Iid == this.foreignParameter.Iid),
                    It.Is<IReadOnlyCollection<Thing>>(c =>
                        c.Count == 1
                        && c.Single() is ParameterSubscription
                        && c.Single().Iid == subscription.Iid
                        && !c.OfType<Parameter>().Any()),
                    It.IsAny<NotificationDescription>()),
                Times.Once);

            Assert.That(this.viewModel.DeleteSubscriptionPopupViewModel.IsVisible, Is.False);
        }

        [Test]
        public async Task VerifyDeleteSubscriptionWithoutTargetIsNoOp()
        {
            await this.viewModel.DeleteSelectedSubscriptionAsync();

            this.sessionService.Verify(
                x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()),
                Times.Never);
        }

        [Test]
        public void VerifyOpenCreateOverridePopupComposesContent()
        {
            this.viewModel.OpenCreateOverridePopup(this.parameter, this.elementUsage);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CreateOverridePopupViewModel.IsVisible, Is.True);
                Assert.That(this.viewModel.CreateOverridePopupViewModel.ContentText, Does.Contain(this.parameter.ParameterType.Name));
                Assert.That(this.viewModel.CreateOverridePopupViewModel.ContentText, Does.Contain(this.elementUsage.Name));
                Assert.That(this.viewModel.CreateOverridePopupViewModel.ContentText, Does.Contain(this.currentDomain.ShortName));
            });
        }

        [Test]
        public void VerifyOpenCreateOverridePopupNullParameterIsNoOp()
        {
            this.viewModel.OpenCreateOverridePopup(null, this.elementUsage);

            Assert.That(this.viewModel.CreateOverridePopupViewModel.IsVisible, Is.False);
        }

        [Test]
        public void VerifyOpenCreateOverridePopupNullHostUsageIsNoOp()
        {
            this.viewModel.OpenCreateOverridePopup(this.parameter, null);

            Assert.That(this.viewModel.CreateOverridePopupViewModel.IsVisible, Is.False);
        }

        [Test]
        public async Task VerifyCreateOverrideCallsSessionServiceWithExpectedPayload()
        {
            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            this.viewModel.OpenCreateOverridePopup(this.parameter, this.elementUsage);
            await this.viewModel.CreateOverrideAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(
                    It.Is<Thing>(t => t is ElementUsage && t.Iid == this.elementUsage.Iid),
                    It.Is<IReadOnlyCollection<Thing>>(c =>
                        c.OfType<ElementUsage>().Any(u => u.Iid == this.elementUsage.Iid)
                        && c.OfType<ParameterOverride>().Any(po => po.Owner != null && po.Owner.Iid == this.currentDomain.Iid && po.Parameter != null && po.Parameter.Iid == this.parameter.Iid)
                        && !c.OfType<ParameterOverrideValueSet>().Any()),
                    It.IsAny<NotificationDescription>()),
                Times.Once);

            Assert.That(this.viewModel.CreateOverridePopupViewModel.IsVisible, Is.False);
        }

        [Test]
        public async Task VerifyCreateOverrideWithoutTargetIsNoOp()
        {
            await this.viewModel.CreateOverrideAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()),
                Times.Never);
        }

        [Test]
        public void VerifyOpenDeleteOverridePopupComposesContent()
        {
            var parameterOverride = new ParameterOverride
            {
                Iid = Guid.NewGuid(),
                Parameter = this.parameter,
                Owner = this.currentDomain
            };

            this.elementUsage.ParameterOverride.Add(parameterOverride);

            this.viewModel.OpenDeleteOverridePopup(parameterOverride);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.DeleteOverridePopupViewModel.IsVisible, Is.True);
                Assert.That(this.viewModel.DeleteOverridePopupViewModel.ContentText, Does.Contain(this.parameter.ParameterType.Name));
                Assert.That(this.viewModel.DeleteOverridePopupViewModel.ContentText, Does.Contain(this.elementUsage.Name));
                Assert.That(this.viewModel.DeleteOverridePopupViewModel.ContentText, Does.Contain("not affected"),
                    "Popup must make it explicit that the source Parameter on the contained ElementDefinition is preserved.");
            });
        }

        [Test]
        public void VerifyOpenDeleteOverridePopupNullIsNoOp()
        {
            this.viewModel.OpenDeleteOverridePopup(null);

            Assert.That(this.viewModel.DeleteOverridePopupViewModel.IsVisible, Is.False);
        }

        [Test]
        public void VerifyOpenDeleteOverridePopupOrphanIsNoOp()
        {
            var orphanOverride = new ParameterOverride
            {
                Iid = Guid.NewGuid(),
                Parameter = this.parameter,
                Owner = this.currentDomain
            };

            this.viewModel.OpenDeleteOverridePopup(orphanOverride);

            Assert.That(this.viewModel.DeleteOverridePopupViewModel.IsVisible, Is.False,
                "An override whose Container is not an ElementUsage cannot be safely deleted from this flow.");
        }

        [Test]
        public async Task VerifyDeleteOverrideDoesNotIncludeParentUsageOrSourceParameterInDeleteList()
        {
            var parameterOverride = new ParameterOverride
            {
                Iid = Guid.NewGuid(),
                Parameter = this.parameter,
                Owner = this.currentDomain
            };

            this.elementUsage.ParameterOverride.Add(parameterOverride);

            this.sessionService
                .Setup(x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            this.viewModel.OpenDeleteOverridePopup(parameterOverride);
            await this.viewModel.DeleteSelectedOverrideAsync();

            this.sessionService.Verify(
                x => x.DeleteThingsWithNotification(
                    It.Is<Thing>(t => t is ElementUsage && t.Iid == this.elementUsage.Iid),
                    It.Is<IReadOnlyCollection<Thing>>(c =>
                        c.Count == 1
                        && c.Single() is ParameterOverride
                        && c.Single().Iid == parameterOverride.Iid
                        && !c.OfType<Parameter>().Any()
                        && !c.OfType<ElementUsage>().Any()),
                    It.IsAny<NotificationDescription>()),
                Times.Once);

            Assert.That(this.viewModel.DeleteOverridePopupViewModel.IsVisible, Is.False);
        }

        [Test]
        public async Task VerifyDeleteOverrideWithoutTargetIsNoOp()
        {
            await this.viewModel.DeleteSelectedOverrideAsync();

            this.sessionService.Verify(
                x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()),
                Times.Never);
        }

        [Test]
        public async Task VerifyAddingElementDefinitionAddsUsageWhenFlagSet()
        {
            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            this.viewModel.AutoAddCreatedDefinitionAsUsage = true;
            this.viewModel.SelectElement(this.referencedElementDefinition);

            var newDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Sensor",
                ShortName = "SEN"
            };

            this.viewModel.ElementDefinitionCreationViewModel.ElementDefinition = newDefinition;

            await this.viewModel.AddingElementDefinitionAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(
                    It.IsAny<Thing>(),
                    It.Is<IReadOnlyCollection<Thing>>(things => things.OfType<ElementUsage>().Any()),
                    It.IsAny<NotificationDescription>()),
                Times.Once,
                "When AutoAddCreatedDefinitionAsUsage is true and a SelectedElementDefinition is set, the things list must contain an ElementUsage.");
        }

        [Test]
        public async Task VerifyAddingElementDefinitionDoesNotAddUsageByDefault()
        {
            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            // AutoAddCreatedDefinitionAsUsage defaults to false — do not set it
            this.viewModel.SelectElement(this.referencedElementDefinition);

            var newDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Actuator",
                ShortName = "ACT"
            };

            this.viewModel.ElementDefinitionCreationViewModel.ElementDefinition = newDefinition;

            await this.viewModel.AddingElementDefinitionAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(
                    It.IsAny<Thing>(),
                    It.Is<IReadOnlyCollection<Thing>>(things => things.OfType<ElementUsage>().Any()),
                    It.IsAny<NotificationDescription>()),
                Times.Never,
                "When AutoAddCreatedDefinitionAsUsage is false (default), no ElementUsage should be included in the things list.");
        }

        [Test]
        public async Task VerifyAddingElementDefinitionUsesNotificationPipeline()
        {
            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            var newDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Gyroscope",
                ShortName = "GYR"
            };

            this.viewModel.ElementDefinitionCreationViewModel.ElementDefinition = newDefinition;

            await this.viewModel.AddingElementDefinitionAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(
                    It.IsAny<Thing>(),
                    It.IsAny<IReadOnlyCollection<Thing>>(),
                    It.Is<NotificationDescription>(n =>
                        n.OnSuccess.Contains("Gyroscope") && n.OnError.Contains("Gyroscope"))),
                Times.Once,
                "AddingElementDefinitionAsync must route through CreateOrUpdateThingsWithNotification so that success/error toasts surface.");

            Assert.That(this.viewModel.IsOnCreationMode, Is.False);
        }

        [Test]
        public void VerifyRefreshSelectedElementClearsDeletedSelection()
        {
            // Select referencedElementDefinition, then remove it from the iteration to simulate a remote deletion.
            this.viewModel.SelectElement(this.referencedElementDefinition);

            Assert.That(this.viewModel.SelectedElement, Is.SameAs(this.referencedElementDefinition));

            this.iteration.Element.Remove(this.referencedElementDefinition);

            this.viewModel.RefreshSelectedElement();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedElement, Is.Null,
                    "When the selected element no longer exists in the iteration, RefreshSelectedElement must clear the selection.");
                Assert.That(this.viewModel.SelectedElementDefinition, Is.Null);
                Assert.That(this.viewModel.ElementDefinitionDetailsViewModel.SelectedSystemNode, Is.Null);
            });
        }

        [Test]
        public void VerifyRefreshSelectedElementClearsDeletedElementUsage()
        {
            // Select the usage, then remove it from its containing definition to simulate a remote deletion.
            this.viewModel.SelectElement(this.elementUsage);

            Assert.That(this.viewModel.SelectedElement, Is.SameAs(this.elementUsage));

            this.topElement.ContainedElement.Remove(this.elementUsage);

            this.viewModel.RefreshSelectedElement();

            Assert.That(this.viewModel.SelectedElement, Is.Null,
                "When the selected Element Usage no longer exists in the iteration, RefreshSelectedElement must clear the selection.");
        }

        [Test]
        public void VerifyRefreshSelectedElementKeepsExistingSelection()
        {
            this.viewModel.SelectElement(this.referencedElementDefinition);

            // Element still in the iteration — refresh must keep it selected and re-build rows.
            this.viewModel.RefreshSelectedElement();

            Assert.That(this.viewModel.SelectedElement, Is.SameAs(this.referencedElementDefinition),
                "When the selected element still exists, RefreshSelectedElement must keep it selected.");
        }
        
        [Test]
        public void VerifyRefreshSelectedElementRaisesIsLoading()
        {
            this.viewModel.SelectElement(this.referencedElementDefinition);

            var isLoadingValues = new List<bool>();
            this.viewModel.WhenAnyValue(x => x.IsLoading).Subscribe(isLoadingValues.Add);

            this.viewModel.RefreshSelectedElement();

            Assert.That(isLoadingValues, Has.Some.EqualTo(true),
                "RefreshSelectedElement rebuilds the rows but never toggles IsLoading, so a host mirroring this " +
                "property (e.g. ModelEditorViewModel.IsLoading) never receives a re-render signal.");
        }

        [Test]
        public void VerifyAvailableParameterGroupsReflectsSelectedElementDefinition()
        {
            var group = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Thermal" };
            this.referencedElementDefinition.ParameterGroup.Add(group);

            this.viewModel.SelectElement(this.referencedElementDefinition);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableParameterGroups, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.AvailableParameterGroups[0].Iid, Is.EqualTo(group.Iid));
            });
        }

        [Test]
        public void VerifyAvailableParameterGroupsIsEmptyWhenNothingSelected()
        {
            Assert.That(this.viewModel.AvailableParameterGroups, Is.Empty);
        }

        [Test]
        public void VerifyOpenCreateParameterGroupPopupSetsEditMode()
        {
            this.viewModel.SelectElement(this.referencedElementDefinition);
            this.viewModel.OpenCreateParameterGroupPopup();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnParameterGroupEditMode, Is.True);
                Assert.That(this.viewModel.ParameterGroupName, Is.EqualTo(string.Empty));
                Assert.That(this.viewModel.ParameterGroupContainingGroup, Is.Null);
            });
        }

        [Test]
        public void VerifyOpenCreateParameterGroupPopupIsNoOpWhenNoElementSelected()
        {
            this.viewModel.OpenCreateParameterGroupPopup();

            Assert.That(this.viewModel.IsOnParameterGroupEditMode, Is.False);
        }

        [Test]
        public void VerifyOpenEditParameterGroupPopupPopulatesForm()
        {
            var group = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Thermal" };
            this.referencedElementDefinition.ParameterGroup.Add(group);
            this.viewModel.SelectElement(this.referencedElementDefinition);

            this.viewModel.OpenEditParameterGroupPopup(group);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnParameterGroupEditMode, Is.True);
                Assert.That(this.viewModel.ParameterGroupName, Is.EqualTo("Thermal"));
                Assert.That(this.viewModel.ParameterGroupContainingGroup, Is.Null);
            });
        }

        [Test]
        public void VerifyOpenEditParameterGroupPopupNullIsNoOp()
        {
            this.viewModel.OpenEditParameterGroupPopup(null);

            Assert.That(this.viewModel.IsOnParameterGroupEditMode, Is.False);
        }

        [Test]
        public async Task VerifyCreateParameterGroupAddsGroupToDefinition()
        {
            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            this.viewModel.SelectElement(this.referencedElementDefinition);
            this.viewModel.OpenCreateParameterGroupPopup();
            this.viewModel.ParameterGroupName = "Thermal";

            await this.viewModel.SaveParameterGroupAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(
                    It.IsAny<Thing>(),
                    It.Is<IReadOnlyCollection<Thing>>(c => c.OfType<ParameterGroup>().Any(g => g.Name == "Thermal")),
                    It.IsAny<NotificationDescription>()),
                Times.Once,
                "SaveParameterGroupAsync must call CreateOrUpdateThingsWithNotification with a ParameterGroup whose Name matches the entered name.");

            Assert.That(this.viewModel.IsOnParameterGroupEditMode, Is.False);
        }

        [Test]
        public async Task VerifyEditParameterGroupUpdatesNameAndContainingGroup()
        {
            var parent = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Parent" };
            var group = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Old" };
            this.referencedElementDefinition.ParameterGroup.Add(parent);
            this.referencedElementDefinition.ParameterGroup.Add(group);
            this.viewModel.SelectElement(this.referencedElementDefinition);

            ParameterGroup captured = null;

            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback<Thing, IReadOnlyCollection<Thing>, NotificationDescription>((_, things, _) => captured = things.OfType<ParameterGroup>().FirstOrDefault(g => g.Iid == group.Iid))
                .ReturnsAsync(Result.Ok());

            this.viewModel.OpenEditParameterGroupPopup(group);
            this.viewModel.ParameterGroupName = "New";
            this.viewModel.ParameterGroupContainingGroup = parent;

            await this.viewModel.SaveParameterGroupAsync();

            Assert.Multiple(() =>
            {
                Assert.That(captured, Is.Not.Null);
                Assert.That(captured.Name, Is.EqualTo("New"));
                Assert.That(captured.ContainingGroup, Is.SameAs(parent));
                Assert.That(this.viewModel.IsOnParameterGroupEditMode, Is.False);
            });
        }

        [Test]
        public async Task VerifySaveParameterGroupRejectsContainingGroupCycle()
        {
            var group = new ParameterGroup { Iid = Guid.NewGuid(), Name = "G" };
            this.referencedElementDefinition.ParameterGroup.Add(group);
            this.viewModel.SelectElement(this.referencedElementDefinition);

            this.viewModel.OpenEditParameterGroupPopup(group);
            this.viewModel.ParameterGroupName = "G";
            this.viewModel.ParameterGroupContainingGroup = group;

            await this.viewModel.SaveParameterGroupAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()),
                Times.Never,
                "Saving a group as its own containing group must be rejected.");

            Assert.That(this.viewModel.IsOnParameterGroupEditMode, Is.False);
        }

        [Test]
        public void VerifyAvailableContainingGroupsExcludesSelfAndDescendants()
        {
            var root = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Root" };
            var child = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Child", ContainingGroup = root };
            var other = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Other" };
            this.referencedElementDefinition.ParameterGroup.Add(root);
            this.referencedElementDefinition.ParameterGroup.Add(child);
            this.referencedElementDefinition.ParameterGroup.Add(other);
            this.viewModel.SelectElement(this.referencedElementDefinition);

            this.viewModel.OpenEditParameterGroupPopup(root);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableContainingGroups, Does.Not.Contain(root));
                Assert.That(this.viewModel.AvailableContainingGroups, Does.Not.Contain(child));
                Assert.That(this.viewModel.AvailableContainingGroups, Does.Contain(other));
            });
        }

        [Test]
        public async Task VerifyCreateParameterGroupWithEmptyNameIsNoOp()
        {
            this.viewModel.SelectElement(this.referencedElementDefinition);
            this.viewModel.OpenCreateParameterGroupPopup();
            this.viewModel.ParameterGroupName = "   ";

            await this.viewModel.SaveParameterGroupAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()),
                Times.Never,
                "SaveParameterGroupAsync must be a no-op when the name is blank.");

            Assert.That(this.viewModel.IsOnParameterGroupEditMode, Is.False);
        }

        [Test]
        public async Task VerifyAssignParameterToGroupSetsGroup()
        {
            var group = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Thermal" };
            this.referencedElementDefinition.ParameterGroup.Add(group);
            this.viewModel.SelectElement(this.referencedElementDefinition);

            Parameter capturedParameter = null;

            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback<Thing, IReadOnlyCollection<Thing>, NotificationDescription>((top, things, _) =>
                {
                    capturedParameter = things.OfType<Parameter>().FirstOrDefault();
                })
                .ReturnsAsync(Result.Ok());

            await this.viewModel.AssignParameterToGroupAsync((this.parameter, group));

            Assert.Multiple(() =>
            {
                Assert.That(capturedParameter, Is.Not.Null);
                Assert.That(capturedParameter.Group, Is.SameAs(group),
                    "The cloned parameter's Group must be set to the supplied ParameterGroup.");
            });
        }

        [Test]
        public async Task VerifyAssignParameterToGroupWithNullGroupUngroupsParameter()
        {
            var group = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Thermal" };
            this.parameter.Group = group;
            this.viewModel.SelectElement(this.referencedElementDefinition);

            Parameter capturedParameter = null;

            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback<Thing, IReadOnlyCollection<Thing>, NotificationDescription>((top, things, _) =>
                {
                    capturedParameter = things.OfType<Parameter>().FirstOrDefault();
                })
                .ReturnsAsync(Result.Ok());

            await this.viewModel.AssignParameterToGroupAsync((this.parameter, null));

            Assert.Multiple(() =>
            {
                Assert.That(capturedParameter, Is.Not.Null);
                Assert.That(capturedParameter.Group, Is.Null,
                    "Passing null as the group must ungroup the parameter (set Group = null on the clone).");
            });
        }

        [Test]
        public async Task VerifyAssignParameterToGroupNullParameterIsNoOp()
        {
            await this.viewModel.AssignParameterToGroupAsync((null, null));

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()),
                Times.Never);
        }

        [Test]
        public async Task VerifyAssignParameterToItsOwnGroupIsNoOp()
        {
            var group = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Thermal" };
            this.referencedElementDefinition.ParameterGroup.Add(group);
            this.parameter.Group = group;
            this.viewModel.SelectElement(this.referencedElementDefinition);

            await this.viewModel.AssignParameterToGroupAsync((this.parameter, group));

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()),
                Times.Never,
                "Assigning a parameter to the group it is already in must not perform a write.");
        }

        [Test]
        public void VerifyOpenDeleteParameterGroupPopupComposesContent()
        {
            var group = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Thermal" };
            this.referencedElementDefinition.ParameterGroup.Add(group);
            this.viewModel.SelectElement(this.referencedElementDefinition);

            this.viewModel.OpenDeleteParameterGroupPopup(group);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.DeleteParameterGroupPopupViewModel.IsVisible, Is.True);
                Assert.That(this.viewModel.DeleteParameterGroupPopupViewModel.ContentText, Does.Contain("Thermal"));
            });
        }

        [Test]
        public void VerifyOpenDeleteParameterGroupPopupNullIsNoOp()
        {
            this.viewModel.OpenDeleteParameterGroupPopup(null);

            Assert.That(this.viewModel.DeleteParameterGroupPopupViewModel.IsVisible, Is.False);
        }

        [Test]
        public async Task VerifyReassignGroupContaining()
        {
            // Build two groups; move groupA under groupB.
            var groupA = new ParameterGroup { Iid = Guid.NewGuid(), Name = "GroupA" };
            var groupB = new ParameterGroup { Iid = Guid.NewGuid(), Name = "GroupB" };

            groupA.Container = this.referencedElementDefinition;
            groupB.Container = this.referencedElementDefinition;

            this.referencedElementDefinition.ParameterGroup.Add(groupA);
            this.referencedElementDefinition.ParameterGroup.Add(groupB);

            this.viewModel.SelectElement(this.referencedElementDefinition);

            ParameterGroup capturedGroupClone = null;

            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback<Thing, IReadOnlyCollection<Thing>, NotificationDescription>((_, things, _) =>
                {
                    capturedGroupClone = things.OfType<ParameterGroup>().FirstOrDefault(g => g.Iid == groupA.Iid);
                })
                .ReturnsAsync(Result.Ok());

            await this.viewModel.ReassignGroupContainingAsync((groupA, groupB));

            Assert.Multiple(() =>
            {
                Assert.That(capturedGroupClone, Is.Not.Null,
                    "CreateOrUpdateThingsWithNotification must be called with a clone of the moved group.");
                Assert.That(capturedGroupClone.ContainingGroup, Is.SameAs(groupB),
                    "The cloned group's ContainingGroup must be set to the target group.");
            });
        }

        [Test]
        public async Task VerifyReassignGroupToSameContainingIsNoOp()
        {
            var groupA = new ParameterGroup { Iid = Guid.NewGuid(), Name = "GroupA" };
            var groupB = new ParameterGroup { Iid = Guid.NewGuid(), Name = "GroupB", ContainingGroup = groupA };

            groupA.Container = this.referencedElementDefinition;
            groupB.Container = this.referencedElementDefinition;

            this.referencedElementDefinition.ParameterGroup.Add(groupA);
            this.referencedElementDefinition.ParameterGroup.Add(groupB);

            this.viewModel.SelectElement(this.referencedElementDefinition);

            // groupB is already under groupA — moving it to groupA again is a no-op.
            await this.viewModel.ReassignGroupContainingAsync((groupB, groupA));

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()),
                Times.Never,
                "Moving a group to the containing group it is already in must not perform a write.");
        }

        [Test]
        public async Task VerifyReassignGroupIntoOwnDescendantIsRejected()
        {
            // groupA contains groupB; trying to move groupA under groupB would create a cycle.
            var groupA = new ParameterGroup { Iid = Guid.NewGuid(), Name = "GroupA" };
            var groupB = new ParameterGroup { Iid = Guid.NewGuid(), Name = "GroupB", ContainingGroup = groupA };

            groupA.Container = this.referencedElementDefinition;
            groupB.Container = this.referencedElementDefinition;

            this.referencedElementDefinition.ParameterGroup.Add(groupA);
            this.referencedElementDefinition.ParameterGroup.Add(groupB);

            this.viewModel.SelectElement(this.referencedElementDefinition);

            await this.viewModel.ReassignGroupContainingAsync((groupA, groupB));

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()),
                Times.Never,
                "Nesting a group under one of its own descendants would create a cycle and must be rejected.");
        }

        [Test]
        public async Task VerifyDeleteParameterGroupClearsReferences()
        {
            // Build a top-level group to delete (ContainingGroup == null → parent is null → children become top-level / ungrouped)
            var groupToDelete = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Thermal" };

            // A child group that references groupToDelete as its ContainingGroup
            var childGroup = new ParameterGroup { Iid = Guid.NewGuid(), Name = "SubThermal", ContainingGroup = groupToDelete };

            // Assign the parameter to groupToDelete
            this.parameter.Group = groupToDelete;

            this.referencedElementDefinition.ParameterGroup.Add(groupToDelete);
            this.referencedElementDefinition.ParameterGroup.Add(childGroup);
            this.viewModel.SelectElement(this.referencedElementDefinition);

            IReadOnlyCollection<Thing> capturedUpdateThings = null;
            IReadOnlyCollection<Thing> capturedDeleteThings = null;

            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback<Thing, IReadOnlyCollection<Thing>, NotificationDescription>((_, things, _) => capturedUpdateThings = things)
                .ReturnsAsync(Result.Ok());

            this.sessionService
                .Setup(x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback<Thing, IReadOnlyCollection<Thing>, NotificationDescription>((_, things, _) => capturedDeleteThings = things)
                .ReturnsAsync(Result.Ok());

            this.viewModel.OpenDeleteParameterGroupPopup(groupToDelete);
            await this.viewModel.DeleteSelectedParameterGroupAsync();

            Assert.Multiple(() =>
            {
                // The update call must include the ungrouped parameter clone (Group == null) because the deleted group was top-level.
                Assert.That(capturedUpdateThings, Is.Not.Null,
                    "An update call must have been issued to clear the references on affected parameters and child groups.");
                Assert.That(capturedUpdateThings.OfType<Parameter>().Any(p => p.Iid == this.parameter.Iid && p.Group == null), Is.True,
                    "The affected parameter's clone must have Group = null in the update list when the deleted group was top-level.");
                Assert.That(capturedUpdateThings.OfType<ParameterGroup>().Any(g => g.Iid == childGroup.Iid && g.ContainingGroup == null), Is.True,
                    "The child group's clone must have ContainingGroup = null in the update list when the deleted group was top-level.");

                // The delete call must target the group being removed
                Assert.That(capturedDeleteThings, Is.Not.Null,
                    "A delete call must have been issued to remove the Parameter Group.");
                Assert.That(capturedDeleteThings.OfType<ParameterGroup>().Any(g => g.Iid == groupToDelete.Iid), Is.True,
                    "The deleted group must be present in the delete things collection.");

                Assert.That(this.viewModel.DeleteParameterGroupPopupViewModel.IsVisible, Is.False);
            });
        }

        [Test]
        public async Task VerifyDeleteNestedGroupMovesChildrenToParent()
        {
            // Arrange: parentGroup → childGroup → grandchildGroup; and parameterInChild belongs to childGroup.
            var parentGroup = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Parent" };
            var childGroup = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Child", ContainingGroup = parentGroup };
            var grandchildGroup = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Grandchild", ContainingGroup = childGroup };

            var parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "Temperature", ShortName = "T" };

            var parameterInChild = new Parameter
            {
                Iid = Guid.NewGuid(),
                Owner = this.currentDomain,
                ParameterType = parameterType,
                Group = childGroup
            };

            this.referencedElementDefinition.Parameter.Add(parameterInChild);
            this.referencedElementDefinition.ParameterGroup.Add(parentGroup);
            this.referencedElementDefinition.ParameterGroup.Add(childGroup);
            this.referencedElementDefinition.ParameterGroup.Add(grandchildGroup);
            this.viewModel.SelectElement(this.referencedElementDefinition);

            IReadOnlyCollection<Thing> capturedUpdateThings = null;

            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback<Thing, IReadOnlyCollection<Thing>, NotificationDescription>((_, things, _) => capturedUpdateThings = things)
                .ReturnsAsync(Result.Ok());

            this.sessionService
                .Setup(x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            // Act: delete childGroup (which has ContainingGroup = parentGroup).
            this.viewModel.OpenDeleteParameterGroupPopup(childGroup);
            await this.viewModel.DeleteSelectedParameterGroupAsync();

            // Assert: parameterInChild must move to parentGroup, grandchildGroup must move to parentGroup.
            Assert.Multiple(() =>
            {
                Assert.That(capturedUpdateThings, Is.Not.Null,
                    "An update call must be issued when there are parameters or child groups to re-parent.");

                var updatedParameter = capturedUpdateThings.OfType<Parameter>().FirstOrDefault(p => p.Iid == parameterInChild.Iid);
                Assert.That(updatedParameter, Is.Not.Null,
                    "The parameter that was in childGroup must appear in the update list.");
                Assert.That(updatedParameter.Group, Is.SameAs(parentGroup),
                    "The parameter must be moved to the parent group, not ungrouped.");

                var updatedGrandchild = capturedUpdateThings.OfType<ParameterGroup>().FirstOrDefault(g => g.Iid == grandchildGroup.Iid);
                Assert.That(updatedGrandchild, Is.Not.Null,
                    "The grandchild group must appear in the update list.");
                Assert.That(updatedGrandchild.ContainingGroup, Is.SameAs(parentGroup),
                    "The grandchild group must be re-parented to parentGroup, not moved to the top level.");
            });
        }

        [Test]
        public void VerifyOpenEditDefinitionPopupSetsIsEditingDefinitionWhenUsageSelected()
        {
            // Select an ElementUsage — OpenEditDefinitionPopup must open for its ElementDefinition.
            this.viewModel.SelectElement(this.elementUsage);
            this.viewModel.OpenEditDefinitionPopup();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnEditMode, Is.True,
                    "OpenEditDefinitionPopup must set IsOnEditMode to true.");
                Assert.That(this.viewModel.IsEditingDefinition, Is.True,
                    "OpenEditDefinitionPopup must set IsEditingDefinition to true.");
                Assert.That(this.viewModel.EditElementDefinitionViewModel.ElementDefinition, Is.Not.Null,
                    "OpenEditDefinitionPopup must have initialised the EditElementDefinitionViewModel with a clone.");
                Assert.That(this.viewModel.EditElementDefinitionViewModel.ElementDefinition.Iid, Is.EqualTo(this.referencedElementDefinition.Iid),
                    "The definition VM must be seeded with the usage's referenced ElementDefinition (by Iid).");
            });
        }

        [Test]
        public void VerifyOpenEditDefinitionPopupIsNoOpWhenNoDefinitionSelected()
        {
            // Nothing selected → OpenEditDefinitionPopup must be a no-op.
            this.viewModel.OpenEditDefinitionPopup();

            Assert.That(this.viewModel.IsOnEditMode, Is.False,
                "OpenEditDefinitionPopup must be a no-op when SelectedElementDefinition is null.");
        }

        [Test]
        public void VerifyOpenEditElementPopupResetIsEditingDefinitionForUsage()
        {
            // OpenEditDefinitionPopup sets IsEditingDefinition = true;
            // a subsequent OpenEditElementPopup (for a usage) must reset it to false.
            this.viewModel.SelectElement(this.elementUsage);
            this.viewModel.OpenEditDefinitionPopup();

            Assert.That(this.viewModel.IsEditingDefinition, Is.True);

            this.viewModel.OpenEditElementPopup();

            Assert.That(this.viewModel.IsEditingDefinition, Is.False,
                "OpenEditElementPopup must set IsEditingDefinition = false when targeting a usage.");
        }

        [Test]
        public async Task VerifyEditElementUsageSetsExcludeOptionFromSelectedOptions()
        {
            // Build an iteration with two options.
            var optionA = new Option { Iid = Guid.NewGuid(), Name = "Option A", ShortName = "OA" };
            var optionB = new Option { Iid = Guid.NewGuid(), Name = "Option B", ShortName = "OB" };
            this.iteration.Option.Add(optionA);
            this.iteration.Option.Add(optionB);

            // Wire the container so ExcludeOption writes reach the clone correctly.
            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            // Select the usage and open the edit popup for it.
            this.viewModel.SelectElement(this.elementUsage);
            this.viewModel.OpenEditElementPopup();

            // AvailableOptions should now have 2 entries; deselect optionB so only optionA is selected.
            this.viewModel.EditElementUsageViewModel.SelectedOptions = [optionA];

            // Execute the save.
            await this.viewModel.EditElementUsageAsync();

            // Verify: ExcludeOption must contain optionB (the complement of the selected set).
            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(
                    It.Is<Thing>(t => t is ElementDefinition && t.Iid == this.topElement.Iid),
                    It.Is<IReadOnlyCollection<Thing>>(c =>
                        c.OfType<ElementUsage>().Any(u =>
                            u.Iid == this.elementUsage.Iid
                            && u.ExcludeOption.Count == 1
                            && u.ExcludeOption.Any(o => o.Iid == optionB.Iid))),
                    It.IsAny<NotificationDescription>()),
                Times.Once,
                "The saved ElementUsage clone must exclude optionB when only optionA is selected.");
        }

        [Test]
        public async Task VerifyDeleteNestedGroupIssuesDeleteInSameInvocation()
        {
            // Arrange: parentGroup → childGroup (to delete); a parameter in childGroup and a grandchild group.
            // The bug was: DeleteThingsWithNotification was called with an already-submitted clone,
            // so the group was never actually deleted on the first call — requiring a second call.
            var parentGroup = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Parent" };
            var childGroup = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Child", ContainingGroup = parentGroup };
            var grandchildGroup = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Grandchild", ContainingGroup = childGroup };

            var parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "Temperature", ShortName = "T" };

            var parameterInChild = new Parameter
            {
                Iid = Guid.NewGuid(),
                Owner = this.currentDomain,
                ParameterType = parameterType,
                Group = childGroup
            };

            this.referencedElementDefinition.Parameter.Add(parameterInChild);
            this.referencedElementDefinition.ParameterGroup.Add(parentGroup);
            this.referencedElementDefinition.ParameterGroup.Add(childGroup);
            this.referencedElementDefinition.ParameterGroup.Add(grandchildGroup);
            this.viewModel.SelectElement(this.referencedElementDefinition);

            var updateCallCount = 0;
            var deleteCallCount = 0;

            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback<Thing, IReadOnlyCollection<Thing>, NotificationDescription>((_, _, _) => updateCallCount++)
                .ReturnsAsync(Result.Ok());

            this.sessionService
                .Setup(x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback<Thing, IReadOnlyCollection<Thing>, NotificationDescription>((_, _, _) => deleteCallCount++)
                .ReturnsAsync(Result.Ok());

            // Act: delete childGroup in a single call.
            this.viewModel.OpenDeleteParameterGroupPopup(childGroup);
            await this.viewModel.DeleteSelectedParameterGroupAsync();

            // Assert: both the reparent update AND the delete must have been issued in this single invocation.
            Assert.Multiple(() =>
            {
                Assert.That(updateCallCount, Is.EqualTo(1),
                    "CreateOrUpdateThingsWithNotification must be called once to reparent the affected parameter and grandchild group.");

                Assert.That(deleteCallCount, Is.EqualTo(1),
                    "DeleteThingsWithNotification must be called once in the same invocation — the bug caused it to be skipped, requiring a second call.");

                this.sessionService.Verify(
                    x => x.DeleteThingsWithNotification(
                        It.Is<Thing>(t => t is ElementDefinition && t.Iid == this.referencedElementDefinition.Iid),
                        It.Is<IReadOnlyCollection<Thing>>(c => c.Count == 1 && c.Single() is ParameterGroup && c.Single().Iid == childGroup.Iid),
                        It.IsAny<NotificationDescription>()),
                    Times.Once,
                    "The delete call must target childGroup as the single thing to delete within the referencedElementDefinition container.");
            });
        }
    }
}
