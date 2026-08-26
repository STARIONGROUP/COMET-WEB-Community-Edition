// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ViewerBodyTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.Viewer
{
    using Bunit;

    using CDP4Common.EngineeringModelData;

    using CDP4Dal;

    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Services.StringTableService;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Components.Viewer;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer;
    using COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel;

    using DynamicData;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Test fixture for <see cref="ViewerBody" /> component.
    /// </summary>
    [TestFixture]
    public class ViewerBodyTestFixture
    {
        /// <summary>
        /// The css class that hides a collapsed panel while keeping it in the render tree.
        /// </summary>
        private const string CollapsedPanelClass = "viewer-panel-collapsed";

        /// <summary>
        /// The <see cref="BunitContext" /> used for rendering components.
        /// </summary>
        private BunitContext context;

        /// <summary>
        /// The <see cref="ICDPMessageBus" /> used for testing.
        /// </summary>
        private CDPMessageBus messageBus;

        /// <summary>
        /// The mock <see cref="ICanvasViewModel" /> used for testing.
        /// </summary>
        private Mock<ICanvasViewModel> canvasViewModel;

        /// <summary>
        /// The mock <see cref="IPropertiesComponentViewModel" /> used for testing.
        /// </summary>
        private Mock<IPropertiesComponentViewModel> propertiesViewModel;

        /// <summary>
        /// Sets up the test context and dependencies before each test execution.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;
            this.context.JSInterop.SetupVoid("DxBlazor.Input.loadModule").SetVoidResult();
            this.context.JSInterop.SetupVoid("DxBlazor.UiHandlersBridge.loadModule").SetVoidResult();

            var sessionService = new Mock<ISessionService>();
            this.messageBus = new CDPMessageBus();

            var mockViewerBodyViewModel = new Mock<IViewerBodyViewModel>();
            var mockOptionSelector = new Mock<IOptionSelectorViewModel>();
            var mockMultipleFiniteStateSelector = new Mock<IMultipleActualFiniteStateSelectorViewModel>();
            mockMultipleFiniteStateSelector.Setup(x => x.ActualFiniteStateSelectorViewModels).Returns([]);
            mockMultipleFiniteStateSelector.Setup(x => x.ActualFiniteStateListsCollection).Returns(new SourceList<ActualFiniteStateList>());

            var selectionMediator = new Mock<ISelectionMediator>();
            var mockBabylonInterop = new Mock<IBabylonInterop>();
            this.canvasViewModel = new Mock<ICanvasViewModel>();
            this.canvasViewModel.Setup(x => x.BabylonInterop).Returns(mockBabylonInterop.Object);
            var mockConfigurationService = new Mock<IConfigurationService>();
            mockConfigurationService.Setup(x => x.ServerConfiguration).Returns(new ServerConfiguration());

            var stringTableService = new Mock<IStringTableService>();
            stringTableService.Setup(x => x.GetText(It.IsAny<string>())).Returns("something");

            this.propertiesViewModel = new Mock<IPropertiesComponentViewModel>();
            var mockDetailsViewModel = new Mock<IDetailsComponentViewModel>();
            this.propertiesViewModel.Setup(x => x.CreateDetailsComponentViewModel()).Returns(mockDetailsViewModel.Object);
            this.propertiesViewModel.Setup(x => x.SelectionMediator).Returns(selectionMediator.Object);

            mockViewerBodyViewModel.Setup(x => x.OptionSelector).Returns(mockOptionSelector.Object);
            mockViewerBodyViewModel.Setup(x => x.MultipleFiniteStateSelector).Returns(mockMultipleFiniteStateSelector.Object);
            mockViewerBodyViewModel.Setup(x => x.ProductTreeViewModel).Returns(new ViewerProductTreeViewModel(selectionMediator.Object));
            mockViewerBodyViewModel.Setup(x => x.CanvasViewModel).Returns(this.canvasViewModel.Object);
            mockViewerBodyViewModel.Setup(x => x.PropertiesViewModel).Returns(this.propertiesViewModel.Object);

            this.context.Services.AddSingleton(sessionService.Object);
            this.context.Services.AddSingleton(mockViewerBodyViewModel.Object);
            this.context.Services.AddSingleton(selectionMediator.Object);
            this.context.Services.AddSingleton(mockConfigurationService.Object);
            this.context.Services.AddSingleton(stringTableService.Object);
            this.context.Services.AddSingleton(this.messageBus);
        }

        /// <summary>
        /// Cleans up the test context after each test execution.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
            this.messageBus.ClearSubscriptions();
        }

        /// <summary>
        /// Verifies that <see cref="ViewerBody" /> renders correctly and updates gridParent CSS class in split view mode.
        /// </summary>
        [Test]
        public void VerifyViewerBodyRendering()
        {
            var renderedComponent = this.context.Render<ViewerBody>();

            var gridParent = renderedComponent.Find("#gridParent");

            var splitViewRenderedComponent = this.context.Render<ViewerBody>(parameters => { parameters.AddCascadingValue(WebAppConstantValues.IsSplitViewCascadingValueName, true); });

            var splitGridParent = splitViewRenderedComponent.Find("#gridParent");

            Assert.Multiple(() =>
            {
                Assert.That(renderedComponent.Instance, Is.Not.Null);
                Assert.That(gridParent, Is.Not.Null);
                Assert.That(renderedComponent.Instance.IsSplitView, Is.False);
                Assert.That(gridParent.ClassList, Does.Not.Contain("split-mode"));
                Assert.That(splitViewRenderedComponent.Instance, Is.Not.Null);
                Assert.That(splitGridParent, Is.Not.Null);
                Assert.That(splitViewRenderedComponent.Instance.IsSplitView, Is.True);
                Assert.That(splitGridParent.ClassList, Does.Contain("split-mode"));
            });
        }

        /// <summary>
        /// Verifies that the product tree and the properties panel can each be collapsed to a strip and restored
        /// independently, and that both panels stay mounted while collapsed (issue GH936).
        /// </summary>
        [Test]
        public void VerifyPanelCollapseAndExpand()
        {
            var renderedComponent = this.context.Render<ViewerBody>();

            Assert.Multiple(() =>
            {
                Assert.That(renderedComponent.Instance.IsProductTreeCollapsed, Is.False);
                Assert.That(renderedComponent.Instance.IsPropertiesPanelCollapsed, Is.False);
                Assert.That(renderedComponent.Find("#leftColumn").ClassList, Does.Not.Contain(CollapsedPanelClass));
                Assert.That(renderedComponent.Find("#rightColumn").ClassList, Does.Not.Contain(CollapsedPanelClass));
                Assert.That(renderedComponent.FindAll(".viewer-collapsed-strip"), Is.Empty);
            });

            renderedComponent.Find("#collapseProductTreePanel").Click();

            renderedComponent.WaitForAssertion(() =>
            {
                Assert.That(renderedComponent.Instance.IsProductTreeCollapsed, Is.True);
                Assert.That(renderedComponent.Find("#leftColumn").ClassList, Does.Contain(CollapsedPanelClass));
                Assert.That(() => renderedComponent.Find("#expandProductTreePanel"), Throws.Nothing);
                Assert.That(renderedComponent.Find("#rightColumn").ClassList, Does.Not.Contain(CollapsedPanelClass),
                    "Collapsing the product tree must not affect the properties panel.");
                Assert.That(renderedComponent.FindComponents<ViewerProductTree>(), Has.Count.EqualTo(1),
                    "A collapsed panel must stay mounted so its view model keeps its subscriptions.");
            });

            renderedComponent.Find("#expandProductTreePanel").Click();

            renderedComponent.WaitForAssertion(() =>
            {
                Assert.That(renderedComponent.Instance.IsProductTreeCollapsed, Is.False);
                Assert.That(renderedComponent.Find("#leftColumn").ClassList, Does.Not.Contain(CollapsedPanelClass));
                Assert.That(() => renderedComponent.Find("#expandProductTreePanel"), Throws.TypeOf<ElementNotFoundException>());
            });

            renderedComponent.Find("#collapsePropertiesPanel").Click();

            renderedComponent.WaitForAssertion(() =>
            {
                Assert.That(renderedComponent.Instance.IsPropertiesPanelCollapsed, Is.True);
                Assert.That(renderedComponent.Find("#rightColumn").ClassList, Does.Contain(CollapsedPanelClass));
                Assert.That(() => renderedComponent.Find("#expandPropertiesPanel"), Throws.Nothing);
                Assert.That(renderedComponent.Find("#leftColumn").ClassList, Does.Not.Contain(CollapsedPanelClass),
                    "Collapsing the properties panel must not affect the product tree.");
            });

            renderedComponent.Find("#expandPropertiesPanel").Click();

            renderedComponent.WaitForAssertion(() =>
            {
                Assert.That(renderedComponent.Instance.IsPropertiesPanelCollapsed, Is.False);
                Assert.That(renderedComponent.Find("#rightColumn").ClassList, Does.Not.Contain(CollapsedPanelClass));
                Assert.That(() => renderedComponent.Find("#expandPropertiesPanel"), Throws.TypeOf<ElementNotFoundException>());
            });
        }

        /// <summary>
        /// Verifies that the collapse chevron of the right column sits in whichever panel is on top: the properties
        /// header once an element is selected, the finite state header while nothing is (issue GH936).
        /// </summary>
        [Test]
        public void VerifyCollapseChevronFollowsTheTopPanel()
        {
            var renderedComponent = this.context.Render<ViewerBody>();

            Assert.Multiple(() =>
            {
                Assert.That(renderedComponent.FindAll("#properties-header"), Is.Empty);
                Assert.That(() => renderedComponent.Find("#state-selector-header #collapsePropertiesPanel"), Throws.Nothing);
            });

            this.propertiesViewModel.Setup(x => x.IsVisible).Returns(true);
            renderedComponent.Render();

            Assert.Multiple(() =>
            {
                Assert.That(() => renderedComponent.Find("#properties-header-actions #collapsePropertiesPanel"), Throws.Nothing);
                Assert.That(renderedComponent.FindAll("#state-selector-header #collapsePropertiesPanel"), Is.Empty);
            });
        }

        /// <summary>
        /// Verifies that both drag-to-resize handles are rendered and that a handle is hidden together with the panel
        /// it resizes (issue GH936).
        /// </summary>
        [Test]
        public void VerifyResizersAreRendered()
        {
            var renderedComponent = this.context.Render<ViewerBody>();

            Assert.Multiple(() =>
            {
                Assert.That(renderedComponent.Find("#left-resizer").ClassList, Does.Not.Contain(CollapsedPanelClass));
                Assert.That(renderedComponent.Find("#right-resizer").ClassList, Does.Not.Contain(CollapsedPanelClass));
            });

            renderedComponent.Find("#collapseProductTreePanel").Click();

            renderedComponent.WaitForAssertion(() =>
            {
                Assert.That(renderedComponent.Find("#left-resizer").ClassList, Does.Contain(CollapsedPanelClass));
                Assert.That(renderedComponent.Find("#right-resizer").ClassList, Does.Not.Contain(CollapsedPanelClass));
            });
        }

        /// <summary>
        /// Verifies that <see cref="ViewerBody" /> re-initializes canvas when <see cref="ViewerBody.IsSplitView" /> transitions from true to false.
        /// </summary>
        [Test]
        public void VerifyOnSplitViewValueChangedHandler()
        {
            var component = this.context.Render<ViewerBody>();

            // Initial component render triggers InitCanvas(true) once via OnAfterRenderAsync
            this.canvasViewModel.Verify(x => x.InitCanvas(true), Times.Once);

            // Changing IsSplitView from false to true should not trigger InitCanvas(true) again
            component.Instance.IsSplitView = true;
            component.Render();
            this.canvasViewModel.Verify(x => x.InitCanvas(true), Times.Once);

            // Changing IsSplitView from true to false should trigger InitCanvas(true) again
            component.Instance.IsSplitView = false;
            component.Render();
            this.canvasViewModel.Verify(x => x.InitCanvas(true), Times.Exactly(2));
        }
    }
}
