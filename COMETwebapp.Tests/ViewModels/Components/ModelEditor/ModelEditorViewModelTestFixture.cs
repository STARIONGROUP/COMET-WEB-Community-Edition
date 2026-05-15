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
    using CDP4Common.Types;

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
            var cacheService = new Mock<ICacheService>();

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
            this.viewModel.EditElementDefinitionViewModel.SelectedCategories = new[] { newCategory };

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
    }
}
