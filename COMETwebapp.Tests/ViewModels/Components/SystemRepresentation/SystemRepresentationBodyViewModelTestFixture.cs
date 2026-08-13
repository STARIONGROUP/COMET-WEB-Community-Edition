// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SystemRepresentationBodyViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.SystemRepresentation
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.Common;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;

    using DynamicData;

    using FluentResults;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class SystemRepresentationBodyViewModelTestFixture
    {
        /// <summary>
        /// The view model under test.
        /// </summary>
        private SystemRepresentationBodyViewModel viewModel;

        /// <summary>
        /// The mocked <see cref="ISessionService" />.
        /// </summary>
        private Mock<ISessionService> sessionService;

        /// <summary>
        /// The mocked <see cref="ISession" />.
        /// </summary>
        private Mock<ISession> session;

        /// <summary>
        /// The mocked <see cref="IPermissionService" /> gating the drag-and-drop move/create pipeline.
        /// </summary>
        private Mock<IPermissionService> permissionService;

        /// <summary>
        /// The message bus used by the view model.
        /// </summary>
        private CDPMessageBus messageBus;

        /// <summary>
        /// The iteration providing the test fixture's element graph.
        /// </summary>
        private Iteration iteration;

        /// <summary>
        /// The iteration's top element.
        /// </summary>
        private ElementDefinition topElement;

        /// <summary>
        /// The currently logged-in <see cref="DomainOfExpertise" />.
        /// </summary>
        private DomainOfExpertise currentDomain;

        /// <summary>
        /// The list of open iterations.
        /// </summary>
        private SourceList<Iteration> openIterations;

        /// <summary>
        /// Builds the iteration graph and the view model with mocked dependencies.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.sessionService = new Mock<ISessionService>();
            this.session = new Mock<ISession>();
            this.sessionService.Setup(x => x.Session).Returns(this.session.Object);
            this.openIterations = new SourceList<Iteration>();
            this.sessionService.Setup(x => x.OpenIterations).Returns(this.openIterations);

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Returns(Task.CompletedTask);

            this.currentDomain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            this.sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(this.currentDomain);

            var rdl = new SiteReferenceDataLibrary { ShortName = "siteRdl", Name = "Site RDL" };
            var siteDirectory = new SiteDirectory { Domain = { this.currentDomain }, SiteReferenceDataLibrary = { rdl } };
            var modelSetup = new EngineeringModelSetup { Name = "ModelName", ShortName = "ModelShortName", ActiveDomain = { this.currentDomain }, RequiredRdl = { new ModelReferenceDataLibrary { RequiredRdl = rdl } } };
            siteDirectory.Model.Add(modelSetup);
            var iterationSetup = new IterationSetup { IterationNumber = 1 };
            modelSetup.IterationSetup.Add(iterationSetup);
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);

            this.topElement = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Container",
                ShortName = "CONT",
                Owner = this.currentDomain
            };

            this.iteration = new Iteration
            {
                Iid = Guid.NewGuid(),
                Element = { this.topElement },
                TopElement = this.topElement,
                IterationSetup = iterationSetup,
                Container = new EngineeringModel { EngineeringModelSetup = modelSetup }
            };

            this.openIterations.Add(this.iteration);

            var panelLogger = new Mock<ILogger<ElementDetailsPanelViewModel>>();
            var panelVm = new ElementDetailsPanelViewModel(this.sessionService.Object, this.messageBus, panelLogger.Object);
            var bodyLogger = new Mock<ILogger<SystemRepresentationBodyViewModel>>();

            this.viewModel = new SystemRepresentationBodyViewModel(this.sessionService.Object, this.messageBus, panelVm, bodyLogger.Object);
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
        public async Task VerifyOnEndUpdateRefreshesPanel()
        {
            // Arrange: initialise the panel directly so we can exercise the refresh path
            // without triggering the ApplyFilters/QueryNestedElements path that requires
            // a non-null Option in the iteration.
            this.viewModel.DetailsPanelViewModel.Initialize(this.iteration);
            this.viewModel.DetailsPanelViewModel.CurrentDomain = this.currentDomain;
            this.viewModel.Elements.Add(this.topElement);

            // Select the top element so the panel has a live selection.
            this.viewModel.SelectElement(new SystemNodeViewModel(this.topElement));

            Assert.That(this.viewModel.DetailsPanelViewModel.ElementDefinitionDetailsViewModel.SelectedSystemNode,
                Is.SameAs(this.topElement),
                "Panel must have the top element selected before the End-Update.");

            // Simulate a write: add a Parameter to the top element (as the session would do after a successful
            // CreateOrUpdate call), then fire the EndUpdate message.
            var parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "Mass", ShortName = "m" };

            var newParameter = new Parameter
            {
                Iid = Guid.NewGuid(),
                Owner = this.currentDomain,
                ParameterType = parameterType
            };

            newParameter.ValueSet.Add(new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                Manual = new ValueArray<string>(["-"]),
                Computed = new ValueArray<string>(["-"]),
                Reference = new ValueArray<string>(["-"]),
                Formula = new ValueArray<string>(["-"]),
                Published = new ValueArray<string>(["-"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            });

            this.topElement.Parameter.Add(newParameter);

            // Send the EndUpdate message — the override OnEndUpdate → OnSessionRefreshed →
            // RefreshSelectedElement path should pick up the new parameter from the live cache.
            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.EndUpdate));

            // Allow any async Rx subscriptions to complete.
            await Task.Delay(100);

            // Assert: the panel rows must now include the newly-added parameter.
            var rows = this.viewModel.DetailsPanelViewModel.ElementDefinitionDetailsViewModel.Rows;

            Assert.That(rows, Is.Not.Null,
                "Rows must not be null after EndUpdate with a selection active.");

            Assert.That(rows.Any(r => r.Parameter.Iid == newParameter.Iid), Is.True,
                "After EndUpdate the panel's rows must include the newly-added parameter, " +
                "proving OnEndUpdate → OnSessionRefreshed → RefreshSelectedElement re-resolves from the live cache.");
        }
        
        [Test]
        public void VerifyOwnParameterOverrideWritePropagatesToOwnPanelAfterEndUpdate()
        {
            var referencedElementDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Box",
                ShortName = "BOX",
                Owner = this.currentDomain
            };

            var parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "Mass", ShortName = "m" };

            var parameter = new Parameter
            {
                Iid = Guid.NewGuid(),
                Owner = this.currentDomain,
                ParameterType = parameterType
            };

            parameter.ValueSet.Add(new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                Manual = new ValueArray<string>(["1"]),
                Computed = new ValueArray<string>(["1"]),
                Reference = new ValueArray<string>(["-"]),
                Formula = new ValueArray<string>(["-"]),
                Published = new ValueArray<string>(["1"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            });

            referencedElementDefinition.Parameter.Add(parameter);

            var elementUsage = new ElementUsage
            {
                Iid = Guid.NewGuid(),
                Name = "Box1",
                ShortName = "BOX1",
                ElementDefinition = referencedElementDefinition,
                Owner = this.currentDomain
            };

            this.topElement.ContainedElement.Add(elementUsage);
            this.iteration.Element.Add(referencedElementDefinition);

            var option = new Option { Iid = Guid.NewGuid(), Name = "Option 1", ShortName = "OPT1" };
            this.iteration.Option.Add(option);
            this.iteration.DefaultOption = option;

            // Genuinely load the iteration, as the real application does, so RefreshProductTree runs for real
            // instead of being skipped because CurrentThing is null.
            this.viewModel.CurrentThing = this.iteration;

            Assert.That(() => this.viewModel.Elements.Any(e => e.Iid == elementUsage.Iid), Is.True.After(2000, 25),
                "the initial load never finished building the product tree");

            var usageNode = this.viewModel.ProductTreeViewModel.RootViewModel.GetFlatListOfDescendants(true)
                .Single(n => n.Thing.Iid == elementUsage.Iid);

            // Select the ElementUsage, as the System Representation product tree does.
            this.viewModel.SelectElement(usageNode);

            Assert.That(this.viewModel.DetailsPanelViewModel.ElementDefinitionDetailsViewModel.SelectedSystemNode,
                Is.SameAs(elementUsage),
                "Panel must have the ElementUsage selected before the write.");

            Assert.That(this.viewModel.DetailsPanelViewModel.ElementDefinitionDetailsViewModel.Rows.Single(r => r.Parameter.Iid == parameter.Iid).HasOverride,
                Is.False,
                "The row must not report an override before one is created.");

            var isLoadingValues = new List<bool>();
            this.viewModel.WhenAnyValue(x => x.IsLoading).Subscribe(isLoadingValues.Add);

            // Simulate the write performed by this panel's own "Create Override" popup: the session updates the
            // live, already-selected ElementUsage in place and notifies the message bus.
            var parameterOverride = new ParameterOverride
            {
                Iid = Guid.NewGuid(),
                Parameter = parameter,
                Owner = this.currentDomain
            };

            elementUsage.ParameterOverride.Add(parameterOverride);

            this.messageBus.SendObjectChangeEvent(parameterOverride, EventKind.Added);
            this.messageBus.SendObjectChangeEvent(elementUsage, EventKind.Updated);
            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.EndUpdate));

            Assert.That(() => isLoadingValues.Contains(true) && !this.viewModel.IsLoading, Is.True.After(2000, 25),
                "the view model never signalled a re-render in reaction to its own write");

            Assert.That(this.viewModel.DetailsPanelViewModel.ElementDefinitionDetailsViewModel.Rows.Single(r => r.Parameter.Iid == parameter.Iid).HasOverride,
                Is.True,
                "After EndUpdate, the panel that performed the ParameterOverride write must show the override on its own rows.");
        }
        
        [Test]
        public async Task VerifyCreateOverrideAsyncOwnFinallyDoesNotClobberEndUpdateRefresh()
        {
            var referencedElementDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Box",
                ShortName = "BOX",
                Owner = this.currentDomain
            };

            var parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "Mass", ShortName = "m" };

            var parameter = new Parameter
            {
                Iid = Guid.NewGuid(),
                Owner = this.currentDomain,
                ParameterType = parameterType
            };

            parameter.ValueSet.Add(new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                Manual = new ValueArray<string>(["1"]),
                Computed = new ValueArray<string>(["1"]),
                Reference = new ValueArray<string>(["-"]),
                Formula = new ValueArray<string>(["-"]),
                Published = new ValueArray<string>(["1"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            });

            referencedElementDefinition.Parameter.Add(parameter);

            var elementUsage = new ElementUsage
            {
                Iid = Guid.NewGuid(),
                Name = "Box1",
                ShortName = "BOX1",
                ElementDefinition = referencedElementDefinition,
                Owner = this.currentDomain
            };

            this.topElement.ContainedElement.Add(elementUsage);
            this.iteration.Element.Add(referencedElementDefinition);

            var option = new Option { Iid = Guid.NewGuid(), Name = "Option 1", ShortName = "OPT1" };
            this.iteration.Option.Add(option);
            this.iteration.DefaultOption = option;

            this.viewModel.CurrentThing = this.iteration;

            Assert.That(() => this.viewModel.Elements.Any(e => e.Iid == elementUsage.Iid), Is.True.After(2000, 25),
                "the initial load never finished building the product tree");

            var usageNode = this.viewModel.ProductTreeViewModel.RootViewModel.GetFlatListOfDescendants(true)
                .Single(n => n.Thing.Iid == elementUsage.Iid);

            this.viewModel.SelectElement(usageNode);

            var parameterOverride = new ParameterOverride
            {
                Iid = Guid.NewGuid(),
                Parameter = parameter,
                Owner = this.currentDomain
            };

            // The mocked write performs its side effects — and publishes the events a real ISession.Write would —
            // from inside the callback, i.e. before CreateOverrideAsync's own await resumes.
            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback(() =>
                {
                    elementUsage.ParameterOverride.Add(parameterOverride);
                    this.messageBus.SendObjectChangeEvent(parameterOverride, EventKind.Added);
                    this.messageBus.SendObjectChangeEvent(elementUsage, EventKind.Updated);
                    this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.EndUpdate));
                })
                .ReturnsAsync(Result.Ok());

            this.viewModel.DetailsPanelViewModel.OpenCreateOverridePopup(parameter, elementUsage);
            await this.viewModel.DetailsPanelViewModel.CreateOverridePopupViewModel.OnConfirm.InvokeAsync();

            Assert.That(() => this.viewModel.DetailsPanelViewModel.ElementDefinitionDetailsViewModel.Rows.Single(r => r.Parameter.Iid == parameter.Iid).HasOverride,
                Is.True.After(2000, 25),
                "After CreateOverrideAsync's own write-and-close sequence completes, the panel that performed it must show the override on its own rows.");
        }
        
        [Test]
        public void VerifyRefreshProductTreeUpdatesRootNodeOnTopElementRename()
        {
            var option = new Option { Iid = Guid.NewGuid(), Name = "Option 1", ShortName = "OPT1" };
            this.iteration.Option.Add(option);
            this.iteration.DefaultOption = option;

            this.viewModel.CurrentThing = this.iteration;

            Assert.That(() => this.viewModel.ProductTreeViewModel.RootViewModel != null, Is.True.After(2000, 25),
                "the initial load never finished building the product tree");

            var rootNode = this.viewModel.ProductTreeViewModel.RootViewModel;
            var originalTitle = rootNode.Title;

            var isLoadingValues = new List<bool>();
            this.viewModel.WhenAnyValue(x => x.IsLoading).Subscribe(isLoadingValues.Add);

            // Rename the top ElementDefinition, as a rename performed by this or another panel would.
            this.topElement.Name = "Renamed Container";

            this.messageBus.SendObjectChangeEvent(this.topElement, EventKind.Updated);
            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.EndUpdate));

            Assert.That(() => isLoadingValues.Contains(true) && !this.viewModel.IsLoading, Is.True.After(2000, 25),
                "the view model never signalled a re-render in reaction to the rename");

            Assert.That(() => rootNode.Title != originalTitle && rootNode.Title == this.topElement.UserFriendlyName, Is.True.After(2000, 25),
                "After EndUpdate, the ROOT node must reflect the renamed top ElementDefinition, " +
                "not just the ElementUsage nodes below it.");
        }

        [Test]
        public void VerifyMovingAUsageRepositionsItInTheProductTree()
        {
            var definitionA = new ElementDefinition { Iid = Guid.NewGuid(), Name = "DefA", ShortName = "DA", Owner = this.currentDomain };
            var definitionB = new ElementDefinition { Iid = Guid.NewGuid(), Name = "DefB", ShortName = "DB", Owner = this.currentDomain };
            var definitionC = new ElementDefinition { Iid = Guid.NewGuid(), Name = "DefC", ShortName = "DC", Owner = this.currentDomain };

            var usageA = new ElementUsage { Iid = Guid.NewGuid(), Name = "UsageA", ShortName = "UA", ElementDefinition = definitionA, Owner = this.currentDomain };
            var usageB = new ElementUsage { Iid = Guid.NewGuid(), Name = "UsageB", ShortName = "UB", ElementDefinition = definitionB, Owner = this.currentDomain };
            var usageC = new ElementUsage { Iid = Guid.NewGuid(), Name = "UsageC", ShortName = "UC", ElementDefinition = definitionC, Owner = this.currentDomain };

            this.topElement.ContainedElement.Add(usageA);
            this.topElement.ContainedElement.Add(usageB);
            definitionA.ContainedElement.Add(usageC);

            this.iteration.Element.Add(definitionA);
            this.iteration.Element.Add(definitionB);
            this.iteration.Element.Add(definitionC);

            var option = new Option { Iid = Guid.NewGuid(), Name = "Option 1", ShortName = "OPT1" };
            this.iteration.Option.Add(option);
            this.iteration.DefaultOption = option;

            this.viewModel.CurrentThing = this.iteration;

            var tree = this.viewModel.ProductTreeViewModel;

            Assert.That(() => tree.RootViewModel != null && tree.RootViewModel.GetFlatListOfDescendants(true).Any(n => n.Thing?.Iid == usageC.Iid),
                Is.True.After(2000, 25),
                "the initial load never drew the usage that is about to be moved");

            // Precondition: usageC is drawn under usageA, which represents definitionA.
            var initialParent = tree.RootViewModel.GetFlatListOfDescendants(true).First(n => n.Thing?.Iid == usageC.Iid).Parent.Thing;
            Assert.That((initialParent as ElementUsage)?.ElementDefinition.Iid, Is.EqualTo(definitionA.Iid),
                "usageC must initially sit under definitionA.");

            var isLoadingValues = new List<bool>();
            this.viewModel.WhenAnyValue(x => x.IsLoading).Subscribe(isLoadingValues.Add);

            // Re-parent usageC from definitionA to definitionB, as ThingCreator.MoveElementUsageAsync and the server would.
            definitionA.ContainedElement.Remove(usageC);
            definitionB.ContainedElement.Add(usageC);

            this.messageBus.SendObjectChangeEvent(usageC, EventKind.Updated);
            this.messageBus.SendObjectChangeEvent(definitionA, EventKind.Updated);
            this.messageBus.SendObjectChangeEvent(definitionB, EventKind.Updated);
            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.EndUpdate));

            Assert.That(() => isLoadingValues.Contains(true) && !this.viewModel.IsLoading, Is.True.After(2000, 25),
                "the view model never signalled a re-render in reaction to the move");

            Assert.That(() =>
                {
                    var node = tree.RootViewModel.GetFlatListOfDescendants(true).FirstOrDefault(n => n.Thing?.Iid == usageC.Iid);
                    var parentThing = node?.Parent?.Thing;
                    var parentDefinition = parentThing as ElementDefinition ?? (parentThing as ElementUsage)?.ElementDefinition;
                    return parentDefinition?.Iid == definitionB.Iid;
                },
                Is.True.After(2000, 25),
                "After the move + EndUpdate, the product tree must redraw usageC under definitionB, not leave it under definitionA.");
        }

        /// <summary>
        /// Verifies that dropping an existing <see cref="ElementUsage" /> node onto a different
        /// <see cref="ElementDefinition" /> node moves (re-parents) the existing usage, rather than creating a
        /// brand-new usage of its referenced definition.
        /// </summary>
        [Test]
        public async Task VerifyDroppingAnElementUsageMovesIt()
        {
            var referencedElementDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Box",
                ShortName = "BOX",
                Owner = this.currentDomain
            };

            var targetElementDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Target",
                ShortName = "TGT",
                Owner = this.currentDomain
            };

            this.iteration.Element.Add(referencedElementDefinition);
            this.iteration.Element.Add(targetElementDefinition);

            var elementUsage = new ElementUsage
            {
                Iid = Guid.NewGuid(),
                Name = "Box1",
                ShortName = "BOX1",
                ElementDefinition = referencedElementDefinition,
                Owner = this.currentDomain
            };

            this.topElement.ContainedElement.Add(elementUsage);

            var option = new Option { Iid = Guid.NewGuid(), Name = "Option 1", ShortName = "OPT1" };
            this.iteration.Option.Add(option);
            this.iteration.DefaultOption = option;

            this.viewModel.CurrentThing = this.iteration;

            Assert.That(() => this.viewModel.CurrentDomain != null, Is.True.After(2000, 25),
                "the current domain must be resolved before the drop can be evaluated");

            OperationContainer capturedOperationContainer = null;

            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>()))
                .Callback<OperationContainer>(operationContainer => capturedOperationContainer = operationContainer)
                .Returns(Task.CompletedTask);

            var fromNode = new SystemNodeViewModel(elementUsage);
            var toNode = new SystemNodeViewModel(targetElementDefinition);

            await this.viewModel.ProductTreeViewModel.OnDrop.InvokeAsync((fromNode, toNode));

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Once);

            var modifiedIids = capturedOperationContainer.Operations.Select(x => x.ModifiedThing.Iid).ToList();

            Assert.That(modifiedIids, Does.Contain(elementUsage.Iid),
                "The existing usage's Iid must be part of the written operation, proving it was moved rather than re-created.");
        }

        /// <summary>
        /// Verifies that dropping an <see cref="ElementDefinition" /> node onto a different
        /// <see cref="ElementDefinition" /> node still creates a new <see cref="ElementUsage" /> of it.
        /// </summary>
        [Test]
        public async Task VerifyDroppingAnElementDefinitionCreatesAUsage()
        {
            var draggedElementDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Box",
                ShortName = "BOX",
                Owner = this.currentDomain
            };

            var targetElementDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Target",
                ShortName = "TGT",
                Owner = this.currentDomain
            };

            this.iteration.Element.Add(draggedElementDefinition);
            this.iteration.Element.Add(targetElementDefinition);

            var option = new Option { Iid = Guid.NewGuid(), Name = "Option 1", ShortName = "OPT1" };
            this.iteration.Option.Add(option);
            this.iteration.DefaultOption = option;

            this.viewModel.CurrentThing = this.iteration;

            Assert.That(() => this.viewModel.CurrentDomain != null, Is.True.After(2000, 25),
                "the current domain must be resolved before the drop can be evaluated");

            var fromNode = new SystemNodeViewModel(draggedElementDefinition);
            var toNode = new SystemNodeViewModel(targetElementDefinition);

            await this.viewModel.ProductTreeViewModel.OnDrop.InvokeAsync((fromNode, toNode));

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Once,
                "Dropping an ElementDefinition onto another one must still create a new ElementUsage.");
        }

        /// <summary>
        /// Verifies that the drop is rejected, without ever writing, when the current user lacks write
        /// permission on the target <see cref="ElementDefinition" />.
        /// </summary>
        [Test]
        public async Task VerifyDropIsRejectedWithoutWritePermission()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(false);

            var referencedElementDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Box",
                ShortName = "BOX",
                Owner = this.currentDomain
            };

            var targetElementDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Target",
                ShortName = "TGT",
                Owner = this.currentDomain
            };

            this.iteration.Element.Add(referencedElementDefinition);
            this.iteration.Element.Add(targetElementDefinition);

            var elementUsage = new ElementUsage
            {
                Iid = Guid.NewGuid(),
                Name = "Box1",
                ShortName = "BOX1",
                ElementDefinition = referencedElementDefinition,
                Owner = this.currentDomain
            };

            this.topElement.ContainedElement.Add(elementUsage);

            var option = new Option { Iid = Guid.NewGuid(), Name = "Option 1", ShortName = "OPT1" };
            this.iteration.Option.Add(option);
            this.iteration.DefaultOption = option;

            this.viewModel.CurrentThing = this.iteration;

            Assert.That(() => this.viewModel.CurrentDomain != null, Is.True.After(2000, 25),
                "the current domain must be resolved before the drop can be evaluated");

            var fromNode = new SystemNodeViewModel(elementUsage);
            var toNode = new SystemNodeViewModel(targetElementDefinition);

            await this.viewModel.ProductTreeViewModel.OnDrop.InvokeAsync((fromNode, toNode));

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never,
                "Without write permission on the target, the drop must be rejected client-side and never reach Write.");
        }
    }
}
