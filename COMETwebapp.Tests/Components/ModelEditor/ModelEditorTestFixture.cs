// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ModelEditorTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.ModelEditor
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4DalCommon.Protocol.Operations;

    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.ViewModels.Components.Common;
    using COMETwebapp.ViewModels.Components.ModelEditor;
    using COMETwebapp.ViewModels.Components.ModelEditor.CopySettings;
    using COMETwebapp.ViewModels.Components.ModelEditor.Rows;

    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using ElementDefinitionTree = COMETwebapp.Components.ModelEditor.ElementDefinitionTree;
    using ModelEditorComponent = COMETwebapp.Components.ModelEditor.ModelEditor;

    /// <summary>
    /// Test fixture for the <see cref="ModelEditorComponent" /> component, focused on the independent
    /// collapse/expand behavior of the source tree panel and the details panel (issue GH742).
    /// </summary>
    [TestFixture]
    public class ModelEditorTestFixture
    {
        /// <summary>
        /// The css class that a minimized panel carries.
        /// </summary>
        private const string CollapsedPanelClass = "model-editor-panel-collapsed";

        /// <summary>
        /// The bunit <see cref="BunitContext" /> used to render the component under test.
        /// </summary>
        private BunitContext context;

        /// <summary>
        /// The mocked <see cref="IModelEditorViewModel" /> bound to the component.
        /// </summary>
        private Mock<IModelEditorViewModel> viewModel;

        /// <summary>
        /// The mocked <see cref="IElementDetailsPanelViewModel" /> exposed by <see cref="viewModel" />.
        /// </summary>
        private Mock<IElementDetailsPanelViewModel> detailsPanelViewModel;

        /// <summary>
        /// The mocked <see cref="ICopySettingsViewModel" /> exposed by <see cref="viewModel" />.
        /// </summary>
        private Mock<ICopySettingsViewModel> copySettingsViewModel;

        /// <summary>
        /// The mocked <see cref="COMETwebapp.ViewModels.Components.ModelEditor.IElementDefinitionTreeViewModel" />
        /// resolved from DI by both nested <see cref="ElementDefinitionTree" /> components.
        /// </summary>
        private Mock<IElementDefinitionTreeViewModel> elementDefinitionTreeViewModel;

        /// <summary>
        /// The <see cref="Iteration" /> exposed as <see cref="IModelEditorViewModel.CurrentThing" />.
        /// </summary>
        private Iteration iteration;

        /// <summary>
        /// Initializes the bunit context and a mocked <see cref="IModelEditorViewModel" /> sufficient to render
        /// the component without a selected element and without the copy-settings popup open.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            var configuration = new Mock<IConfigurationService>();
            configuration.Setup(x => x.ServerConfiguration).Returns(new ServerConfiguration());
            this.context.Services.AddSingleton(configuration.Object);

            this.elementDefinitionTreeViewModel = new Mock<IElementDefinitionTreeViewModel>();
            this.elementDefinitionTreeViewModel.Setup(x => x.Rows).Returns([]);
            this.elementDefinitionTreeViewModel.Setup(x => x.Iterations).Returns([]);
            this.context.Services.AddSingleton(this.elementDefinitionTreeViewModel.Object);

            this.iteration = new Iteration
            {
                Iid = Guid.NewGuid(),
                IterationSetup = new IterationSetup
                {
                    IterationNumber = 1,
                    Container = new EngineeringModelSetup
                    {
                        Iid = Guid.NewGuid(),
                        Name = "ModelName",
                        ShortName = "ModelShortName"
                    }
                }
            };

            this.detailsPanelViewModel = new Mock<IElementDetailsPanelViewModel>();
            this.detailsPanelViewModel.Setup(x => x.SelectedElementDefinition).Returns((ElementDefinition)null);

            this.copySettingsViewModel = new Mock<ICopySettingsViewModel>();
            this.copySettingsViewModel.Setup(x => x.AvailableOperationKinds).Returns(new CopyOperationKinds());
            this.copySettingsViewModel.Setup(x => x.SelectedOperationKind).Returns(OperationKind.Copy);

            this.viewModel = new Mock<IModelEditorViewModel>();
            this.viewModel.Setup(x => x.IsLoading).Returns(false);
            this.viewModel.Setup(x => x.CurrentThing).Returns(this.iteration);
            this.viewModel.Setup(x => x.IsSourceModelSameAsTargetModel).Returns(true);
            this.viewModel.Setup(x => x.IsOnCopySettingsMode).Returns(false);
            this.viewModel.Setup(x => x.CopySettingsViewModel).Returns(this.copySettingsViewModel.Object);
            this.viewModel.Setup(x => x.DetailsPanelViewModel).Returns(this.detailsPanelViewModel.Object);

            // ApplicationBase<TViewModel>.InjectedViewModel is [Inject] and must be resolvable from DI even
            // though the same instance is also supplied explicitly as ParameterizedViewModel below.
            this.context.Services.AddSingleton(this.viewModel.Object);
        }

        /// <summary>
        /// Disposes of the bunit context.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        /// <summary>
        /// Renders the <see cref="ModelEditorComponent" /> bound to <see cref="viewModel" />.
        /// </summary>
        /// <returns>The rendered <see cref="ModelEditorComponent" />.</returns>
        private IRenderedComponent<ModelEditorComponent> RenderModelEditor()
        {
            return this.context.Render<ModelEditorComponent>(parameters =>
                parameters.Add(p => p.ParameterizedViewModel, this.viewModel.Object));
        }

        [Test]
        public void VerifyComponent()
        {
            var renderedComponent = this.RenderModelEditor();

            Assert.Multiple(() =>
            {
                Assert.That(renderedComponent, Is.Not.Null);
                Assert.That(renderedComponent.Instance, Is.Not.Null);
                Assert.That(renderedComponent.Instance.ViewModel, Is.EqualTo(this.viewModel.Object));
            });
        }

        /// <summary>
        /// Verifies that the iteration of both trees reaches the ViewModel, which is what decides whether a copy between the two
        /// panels crosses iterations. The trees are captured with @ref and are therefore still null when the parameters are set,
        /// so subscribing to them any earlier than the first render leaves the ViewModel believing the panels hold different
        /// iterations, and the Model Editor then wrongly announces the different-Iteration copy mode.
        /// </summary>
        [Test]
        public void VerifyTreeIterationsReachTheViewModel()
        {
            this.elementDefinitionTreeViewModel.Setup(x => x.Iteration).Returns(this.iteration);

            var renderedComponent = this.RenderModelEditor();

            renderedComponent.WaitForAssertion(() =>
            {
                this.viewModel.VerifySet(x => x.SourceIteration = this.iteration, Times.AtLeastOnce);
                this.viewModel.VerifySet(x => x.TargetIteration = this.iteration, Times.AtLeastOnce);
            });
        }
        
        [Test]
        public async Task VerifyDragAndDropUsesTheModifierKeyOperationKind()
        {
            var owner = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS" };
            var draggedElement = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Dragged", Owner = owner };
            var targetElement = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Target", Owner = owner };
            this.iteration.Element.Add(draggedElement);
            this.iteration.Element.Add(targetElement);

            var draggedRow = new ElementDefinitionTreeRowViewModel(draggedElement);
            var targetRow = new ElementDefinitionTreeRowViewModel(targetElement);

            this.elementDefinitionTreeViewModel.Setup(x => x.Iteration).Returns(this.iteration);

            var renderedComponent = this.RenderModelEditor();
            var trees = renderedComponent.FindComponents<ElementDefinitionTree>();
            var sourceTree = trees[0].Instance;
            var targetTree = trees[1].Instance;

            // Nothing is being dragged yet, so no drop is allowed.
            await renderedComponent.InvokeAsync(() => targetTree.OnCalculateDropIsAllowed.InvokeAsync(targetTree));
            Assert.That(targetTree.AllowNodeDrop, Is.False);

            await renderedComponent.InvokeAsync(() => sourceTree.OnDragStart.InvokeAsync((sourceTree, draggedRow)));
            await renderedComponent.InvokeAsync(() => targetTree.OnDragEnter.InvokeAsync((targetTree, null)));
            await renderedComponent.InvokeAsync(() => targetTree.OnCalculateDropIsAllowed.InvokeAsync(targetTree));

            Assert.That(targetTree.AllowNodeDrop, Is.True, "Dragging an ElementDefinition over the target tree must allow the drop.");

            // Ctrl + Shift maps to CopyKeepValues: keep the parameter values and the original owner.
            await renderedComponent.InvokeAsync(() => targetTree.OnDrop.InvokeAsync((targetTree, null, new DragEventArgs { CtrlKey = true, ShiftKey = true })));

            this.viewModel.Verify(x => x.CopyAndAddNewElementAsync(targetTree, draggedElement, OperationKind.CopyKeepValues), Times.Once);

            // Without a modifier key the copy mode selected in the copy settings applies, which the ViewModel resolves itself.
            await renderedComponent.InvokeAsync(() => sourceTree.OnDragStart.InvokeAsync((sourceTree, draggedRow)));
            await renderedComponent.InvokeAsync(() => targetTree.OnDrop.InvokeAsync((targetTree, null, new DragEventArgs())));

            this.viewModel.Verify(x => x.CopyAndAddNewElementAsync(targetTree, draggedElement, null), Times.Once);

            // Dropping onto a node adds an ElementUsage rather than copying.
            await renderedComponent.InvokeAsync(() => sourceTree.OnDragStart.InvokeAsync((sourceTree, draggedRow)));
            await renderedComponent.InvokeAsync(() => targetTree.OnDrop.InvokeAsync((targetTree, targetRow, new DragEventArgs())));

            this.viewModel.Verify(x => x.AddNewElementUsageAsync(draggedElement, targetElement), Times.Once);

            await renderedComponent.InvokeAsync(() => targetTree.OnDragLeave.InvokeAsync((targetTree, null)));
            await renderedComponent.InvokeAsync(() => sourceTree.OnDragEnd.InvokeAsync((sourceTree, draggedRow)));

            // The drag is over, so a drop is no longer allowed.
            await renderedComponent.InvokeAsync(() => targetTree.OnCalculateDropIsAllowed.InvokeAsync(targetTree));
            Assert.That(targetTree.AllowNodeDrop, Is.False);
        }

        /// <summary>
        /// Verifies that a failing copy is reported to the user instead of being swallowed
        /// </summary>
        [Test]
        public async Task VerifyDropReportsAFailingCopy()
        {
            var owner = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS" };
            var draggedElement = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Dragged", Owner = owner };
            this.iteration.Element.Add(draggedElement);
            var draggedRow = new ElementDefinitionTreeRowViewModel(draggedElement);

            this.elementDefinitionTreeViewModel.Setup(x => x.Iteration).Returns(this.iteration);

            this.viewModel.Setup(x => x.CopyAndAddNewElementAsync(It.IsAny<ElementDefinitionTree>(), It.IsAny<ElementBase>(), It.IsAny<OperationKind?>()))
                .ThrowsAsync(new InvalidOperationException("copy refused"));

            var renderedComponent = this.RenderModelEditor();
            var trees = renderedComponent.FindComponents<ElementDefinitionTree>();
            var sourceTree = trees[0].Instance;
            var targetTree = trees[1].Instance;

            await renderedComponent.InvokeAsync(() => sourceTree.OnDragStart.InvokeAsync((sourceTree, draggedRow)));
            await renderedComponent.InvokeAsync(() => targetTree.OnDrop.InvokeAsync((targetTree, null, new DragEventArgs())));

            renderedComponent.WaitForAssertion(() => Assert.That(renderedComponent.Markup, Does.Contain("copy refused")));
        }

        /// <summary>
        /// Verifies that selecting a node in either tree drives the details panel, and clears the selection of the other tree
        /// </summary>
        [Test]
        public async Task VerifySelectingANodeDrivesTheDetailsPanel()
        {
            var owner = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS" };
            var elementDefinition = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Selected", Owner = owner };
            this.iteration.Element.Add(elementDefinition);
            var row = new ElementDefinitionTreeRowViewModel(elementDefinition);

            var renderedComponent = this.RenderModelEditor();
            var trees = renderedComponent.FindComponents<ElementDefinitionTree>();

            await renderedComponent.InvokeAsync(() => trees[0].Instance.SelectionChanged.InvokeAsync(row));
            this.detailsPanelViewModel.Verify(x => x.SelectElement(elementDefinition), Times.Once);

            await renderedComponent.InvokeAsync(() => trees[1].Instance.SelectionChanged.InvokeAsync(null));
            this.detailsPanelViewModel.Verify(x => x.SelectElement(null), Times.Once);
        }

        [Test]
        public void VerifyPanelCollapseAndExpand()
        {
            var renderedComponent = this.RenderModelEditor();

            Assert.Multiple(() =>
            {
                Assert.That(renderedComponent.Instance.IsSourcePanelCollapsed, Is.False);
                Assert.That(renderedComponent.Instance.IsDetailsPanelCollapsed, Is.False);
                Assert.That(renderedComponent.Find("#sourcePanel").ClassList, Does.Not.Contain(CollapsedPanelClass));
                Assert.That(renderedComponent.Find("#detailsPanel").ClassList, Does.Not.Contain(CollapsedPanelClass));
                Assert.That(renderedComponent.FindAll(".model-editor-collapsed-strip"), Is.Empty);
            });

            // Collapse the source panel: only the source panel is affected.
            renderedComponent.Find("#collapseSourcePanel").Click();

            renderedComponent.WaitForAssertion(() =>
            {
                Assert.That(renderedComponent.Instance.IsSourcePanelCollapsed, Is.True);
                Assert.That(renderedComponent.Find("#sourcePanel").ClassList, Does.Contain(CollapsedPanelClass));
                Assert.That(() => renderedComponent.Find("#expandSourcePanel"), Throws.Nothing);
                Assert.That(renderedComponent.Find("#detailsPanel").ClassList, Does.Not.Contain(CollapsedPanelClass),
                    "Collapsing the source panel must not affect the details panel.");
            });

            // Re-expand the source panel.
            renderedComponent.Find("#expandSourcePanel").Click();

            renderedComponent.WaitForAssertion(() =>
            {
                Assert.That(renderedComponent.Instance.IsSourcePanelCollapsed, Is.False);
                Assert.That(renderedComponent.Find("#sourcePanel").ClassList, Does.Not.Contain(CollapsedPanelClass));
                Assert.That(() => renderedComponent.Find("#expandSourcePanel"), Throws.TypeOf<ElementNotFoundException>());
            });

            // Collapse the details panel: only the details panel is affected.
            renderedComponent.Find("#collapseDetailsPanel").Click();

            renderedComponent.WaitForAssertion(() =>
            {
                Assert.That(renderedComponent.Instance.IsDetailsPanelCollapsed, Is.True);
                Assert.That(renderedComponent.Find("#detailsPanel").ClassList, Does.Contain(CollapsedPanelClass));
                Assert.That(() => renderedComponent.Find("#expandDetailsPanel"), Throws.Nothing);
                Assert.That(renderedComponent.Instance.IsSourcePanelCollapsed, Is.False,
                    "Collapsing the details panel must not affect the source panel.");
                Assert.That(renderedComponent.Find("#sourcePanel").ClassList, Does.Not.Contain(CollapsedPanelClass));
            });

            // Re-expand the details panel.
            renderedComponent.Find("#expandDetailsPanel").Click();

            renderedComponent.WaitForAssertion(() =>
            {
                Assert.That(renderedComponent.Instance.IsDetailsPanelCollapsed, Is.False);
                Assert.That(renderedComponent.Find("#detailsPanel").ClassList, Does.Not.Contain(CollapsedPanelClass));
                Assert.That(() => renderedComponent.Find("#expandDetailsPanel"), Throws.TypeOf<ElementNotFoundException>());
            });
        }

        /// <summary>
        /// Verifies that collapsing a panel only hides it, and never unmounts its <see cref="ElementDefinitionTree" />.
        /// An unmounted tree would discard its transient ViewModel without disposing the message bus and
        /// DynamicData subscriptions that it owns, leaking them on every collapse.
        /// </summary>
        [Test]
        public void VerifyCollapsedPanelsStayMounted()
        {
            var renderedComponent = this.RenderModelEditor();

            Assert.That(renderedComponent.FindComponents<ElementDefinitionTree>(), Has.Count.EqualTo(2));

            renderedComponent.Find("#collapseSourcePanel").Click();

            renderedComponent.WaitForAssertion(() =>
            {
                Assert.That(renderedComponent.Instance.IsSourcePanelCollapsed, Is.True);
                Assert.That(renderedComponent.FindComponents<ElementDefinitionTree>(), Has.Count.EqualTo(2),
                    "A collapsed source panel must stay mounted, so that its ViewModel's subscriptions are not leaked.");
            });
        }
    }
}
