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
    using System.Collections.Concurrent;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4Web.Enumerations;

    using COMET.Web.Common.Services.Cache;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.Common;
    using COMETwebapp.ViewModels.Components.ModelEditor;

    using DynamicData;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

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
        /// The iteration's top element.
        /// </summary>
        private ElementDefinition topElement;

        /// <summary>
        /// The currently logged-in <see cref="DomainOfExpertise" />.
        /// </summary>
        private DomainOfExpertise currentDomain;
        private Mock<ISession> session;

        /// <summary>
        /// The mocked <see cref="IPermissionService" /> used to gate <see cref="ModelEditorViewModel.MoveElementUsageAsync" />.
        /// </summary>
        private Mock<IPermissionService> permissionService;

        /// <summary>
        /// Builds the iteration graph and the view model with mocked dependencies.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.sessionService = new Mock<ISessionService>();
            var cacheService = new Mock<ICacheService>();

            this.session = new Mock<ISession>();
            this.sessionService.Setup(x => x.Session).Returns(this.session.Object);
            this.sessionService.Setup(x => x.OpenIterations).Returns(new SourceList<Iteration>());

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Returns(Task.CompletedTask);

            var domain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            this.currentDomain = domain;
            this.sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(domain);

            var rdl = new SiteReferenceDataLibrary { ShortName = "siteRdl", Name = "Site RDL" };
            var siteDirectory = new SiteDirectory { Domain = { domain }, SiteReferenceDataLibrary = { rdl } };
            var modelSetup = new EngineeringModelSetup { Name = "ModelName", ShortName = "ModelShortName", ActiveDomain = { domain }, RequiredRdl = { new ModelReferenceDataLibrary { RequiredRdl = rdl } } };
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
                Owner = domain
            };

            this.iteration = new Iteration
            {
                Iid = Guid.NewGuid(),
                Element = { this.topElement },
                TopElement = this.topElement,
                IterationSetup = iterationSetup,
                Container = new EngineeringModel { EngineeringModelSetup = modelSetup }
            };

            var panelLogger = new Mock<ILogger<ElementDetailsPanelViewModel>>();
            var panelVm = new ElementDetailsPanelViewModel(this.sessionService.Object, this.messageBus, panelLogger.Object);

            this.viewModel = new ModelEditorViewModel(this.sessionService.Object, this.messageBus, cacheService.Object, panelVm)
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
        public void VerifyDetailsPanelIterationIsPropagatedOnThingChanged()
        {
            // OnThingChanged fires when CurrentThing is set in SetUp; by the time we reach here the
            // panel's CurrentIteration should already equal the iteration.
            Assert.That(this.viewModel.DetailsPanelViewModel.CurrentIteration, Is.SameAs(this.iteration),
                "ModelEditorViewModel.OnThingChanged must propagate CurrentThing to DetailsPanelViewModel.CurrentIteration via Initialize.");
        }

        [Test]
        public void VerifyOpenCopySettingsPopupSetsMode()
        {
            this.viewModel.OpenCopySettingsPopup();

            Assert.That(this.viewModel.IsOnCopySettingsMode, Is.True);
        }

        [Test]
        public void VerifySourceIterationEqualsTargetIterationSetsIsSourceModelSameAsTargetModel()
        {
            this.viewModel.SourceIteration = this.iteration;
            this.viewModel.TargetIteration = this.iteration;

            Assert.That(this.viewModel.IsSourceModelSameAsTargetModel, Is.True);
        }
        
        [Test]
        public void VerifyIsSourceModelSameAsTargetModel()
        {
            var otherIteration = new Iteration { Iid = Guid.NewGuid() };

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsSourceModelSameAsTargetModel, Is.False, "No source iteration is selected yet.");

                this.viewModel.SourceIteration = otherIteration;
                this.viewModel.TargetIteration = this.iteration;
                Assert.That(this.viewModel.IsSourceModelSameAsTargetModel, Is.False);

                this.viewModel.SourceIteration = this.iteration;
                Assert.That(this.viewModel.IsSourceModelSameAsTargetModel, Is.True);

                this.viewModel.SourceIteration = null;
                Assert.That(this.viewModel.IsSourceModelSameAsTargetModel, Is.False);
            });
        }

        /// <summary>
        /// Verifies that the copy and the element usage entry points refuse null arguments, rather than failing later inside the
        /// SDK copy machinery where the cause would be much harder to see
        /// </summary>
        [Test]
        public void VerifyCopyAndAddNewElementAsyncGuardsItsArguments()
        {
            var elementDefinition = new ElementDefinition { Iid = Guid.NewGuid(), Owner = this.currentDomain };

            Assert.Multiple(() =>
            {
                Assert.That(async () => await this.viewModel.CopyAndAddNewElementAsync(null, elementDefinition),
                    Throws.TypeOf<ArgumentNullException>());

                Assert.That(async () => await this.viewModel.AddNewElementUsageAsync(null, elementDefinition),
                    Throws.TypeOf<ArgumentNullException>());

                Assert.That(async () => await this.viewModel.AddNewElementUsageAsync(elementDefinition, null),
                    Throws.TypeOf<ArgumentNullException>());
            });
        }

        [Test]
        public async Task VerifyExternalParameterChangeIsReflectedAfterEndUpdate()
        {
            this.viewModel.DetailsPanelViewModel.SelectElement(this.topElement);

            var isLoadingValues = new List<bool>();
            this.viewModel.WhenAnyValue(x => x.IsLoading).Subscribe(isLoadingValues.Add);

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

            // Simulate the other panel's write reaching this session's message bus.
            this.messageBus.SendObjectChangeEvent(this.topElement, EventKind.Updated);
            this.messageBus.SendMessage(new SessionEvent(this.sessionService.Object.Session, SessionStatus.EndUpdate));

            Assert.That(() => isLoadingValues.Contains(true) && !this.viewModel.IsLoading, Is.True.After(2000, 25),
                "the view model never signalled a re-render in reaction to the external write");

            var rows = this.viewModel.DetailsPanelViewModel.ElementDefinitionDetailsViewModel.Rows;

            Assert.Multiple(() =>
            {
                Assert.That(rows.Any(r => r.Parameter.Iid == newParameter.Iid), Is.True,
                    "ModelEditorViewModel.OnEndUpdate must refresh the details panel so a cross-panel Parameter write becomes visible.");

                Assert.That(isLoadingValues, Has.Some.EqualTo(true),
                    "IsLoading must toggle so that the Blazor component subscribed to it knows to re-render.");
            });
        }

        [Test]
        public async Task VerifyMoveElementUsageWritesWhenValid()
        {
            var cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            var movingIteration = new Iteration(Guid.NewGuid(), cache, null) { Container = new EngineeringModel { Iid = Guid.NewGuid() } };
            var sourceElementDefinition = new ElementDefinition(Guid.NewGuid(), cache, null);
            var targetElementDefinition = new ElementDefinition(Guid.NewGuid(), cache, null);
            var referencedElementDefinition = new ElementDefinition(Guid.NewGuid(), cache, null);

            movingIteration.Element.Add(sourceElementDefinition);
            movingIteration.Element.Add(targetElementDefinition);
            movingIteration.Element.Add(referencedElementDefinition);

            var elementUsage = new ElementUsage(Guid.NewGuid(), cache, null)
            {
                Owner = this.currentDomain,
                ElementDefinition = referencedElementDefinition
            };

            sourceElementDefinition.ContainedElement.Add(elementUsage);

            OperationContainer capturedOperationContainer = null;

            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>()))
                .Callback<OperationContainer>(operationContainer => capturedOperationContainer = operationContainer)
                .Returns(Task.CompletedTask);

            await this.viewModel.MoveElementUsageAsync(elementUsage, targetElementDefinition);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Once);
            Assert.That(capturedOperationContainer, Is.Not.Null);
        }

        /// <summary>
        /// Verifies that <see cref="ModelEditorViewModel.MoveElementUsageAsync" /> silently refuses the move when
        /// it would cross iterations, is a no-op (target equals the current container), would introduce a
        /// containment cycle, or the current user lacks write permission on the target.
        /// </summary>
        [Test]
        public async Task VerifyMoveElementUsageIsRejected()
        {
            // (a) cross-iteration: the target container belongs to a different Iteration.
            var cacheA = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            var iterationA1 = new Iteration(Guid.NewGuid(), cacheA, null);
            var iterationA2 = new Iteration(Guid.NewGuid(), cacheA, null);
            var sourceA = new ElementDefinition(Guid.NewGuid(), cacheA, null);
            var targetA = new ElementDefinition(Guid.NewGuid(), cacheA, null);
            var referencedA = new ElementDefinition(Guid.NewGuid(), cacheA, null);
            iterationA1.Element.Add(sourceA);
            iterationA1.Element.Add(referencedA);
            iterationA2.Element.Add(targetA);

            var usageA = new ElementUsage(Guid.NewGuid(), cacheA, null) { Owner = this.currentDomain, ElementDefinition = referencedA };
            sourceA.ContainedElement.Add(usageA);

            await this.viewModel.MoveElementUsageAsync(usageA, targetA);

            // (b) already contained: the target equals the usage's current container.
            var cacheB = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            var iterationB = new Iteration(Guid.NewGuid(), cacheB, null);
            var sourceB = new ElementDefinition(Guid.NewGuid(), cacheB, null);
            var referencedB = new ElementDefinition(Guid.NewGuid(), cacheB, null);
            iterationB.Element.Add(sourceB);
            iterationB.Element.Add(referencedB);

            var usageB = new ElementUsage(Guid.NewGuid(), cacheB, null) { Owner = this.currentDomain, ElementDefinition = referencedB };
            sourceB.ContainedElement.Add(usageB);

            await this.viewModel.MoveElementUsageAsync(usageB, sourceB);

            // (c) containment cycle: the usage's referenced definition already (transitively) contains the target.
            var cacheC = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            var iterationC = new Iteration(Guid.NewGuid(), cacheC, null);
            var sourceC = new ElementDefinition(Guid.NewGuid(), cacheC, null);
            var referencedC = new ElementDefinition(Guid.NewGuid(), cacheC, null);
            var targetC = new ElementDefinition(Guid.NewGuid(), cacheC, null);
            iterationC.Element.Add(sourceC);
            iterationC.Element.Add(referencedC);
            iterationC.Element.Add(targetC);

            var innerUsageC = new ElementUsage(Guid.NewGuid(), cacheC, null) { Owner = this.currentDomain, ElementDefinition = targetC };
            referencedC.ContainedElement.Add(innerUsageC);

            var usageC = new ElementUsage(Guid.NewGuid(), cacheC, null) { Owner = this.currentDomain, ElementDefinition = referencedC };
            sourceC.ContainedElement.Add(usageC);

            await this.viewModel.MoveElementUsageAsync(usageC, targetC);

            // (d) no write permission on the target.
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(false);

            var cacheD = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            var iterationD = new Iteration(Guid.NewGuid(), cacheD, null);
            var sourceD = new ElementDefinition(Guid.NewGuid(), cacheD, null);
            var targetD = new ElementDefinition(Guid.NewGuid(), cacheD, null);
            var referencedD = new ElementDefinition(Guid.NewGuid(), cacheD, null);
            iterationD.Element.Add(sourceD);
            iterationD.Element.Add(targetD);
            iterationD.Element.Add(referencedD);

            var usageD = new ElementUsage(Guid.NewGuid(), cacheD, null) { Owner = this.currentDomain, ElementDefinition = referencedD };
            sourceD.ContainedElement.Add(usageD);

            await this.viewModel.MoveElementUsageAsync(usageD, targetD);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never);
        }

        [Test]
        public void VerifyCanWriteElementUsageReflectsPermission()
        {
            var targetContainer = new ElementDefinition { Iid = Guid.NewGuid(), Owner = this.currentDomain };

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            Assert.That(this.viewModel.CanWriteElementUsage(targetContainer), Is.True);

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(false);
            Assert.That(this.viewModel.CanWriteElementUsage(targetContainer), Is.False);

            Assert.That(() => this.viewModel.CanWriteElementUsage(null), Throws.TypeOf<ArgumentNullException>());
        }
    }
}
