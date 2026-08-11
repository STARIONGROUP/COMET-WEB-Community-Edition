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
    using System.Reactive.Subjects;

    using Bunit;

    using CDP4Dal;

    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Services.StringTableService;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Components.Viewer;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer;
    using COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel;

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
        /// Sets up the test context and dependencies before each test execution.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            var sessionService = new Mock<ISessionService>();
            this.messageBus = new CDPMessageBus();

            var mockViewerBodyViewModel = new Mock<IViewerBodyViewModel>();
            var mockOptionSelector = new Mock<IOptionSelectorViewModel>();
            var mockMultipleFiniteStateSelector = new Mock<IMultipleActualFiniteStateSelectorViewModel>();
            mockMultipleFiniteStateSelector.Setup(x => x.ActualFiniteStateSelectorViewModels).Returns([]);

            var selectionMediator = new Mock<ISelectionMediator>();
            this.canvasViewModel = new Mock<ICanvasViewModel>();
            var mockConfigurationService = new Mock<IConfigurationService>();
            mockConfigurationService.Setup(x => x.ServerConfiguration).Returns(new ServerConfiguration());

            var stringTableService = new Mock<IStringTableService>();
            stringTableService.Setup(x => x.GetText(It.IsAny<string>())).Returns("something");

            var mockPropertiesViewModel = new Mock<IPropertiesComponentViewModel>();
            var mockDetailsViewModel = new Mock<IDetailsComponentViewModel>();
            mockPropertiesViewModel.Setup(x => x.CreateDetailsComponentViewModel()).Returns(mockDetailsViewModel.Object);

            mockViewerBodyViewModel.Setup(x => x.OptionSelector).Returns(mockOptionSelector.Object);
            mockViewerBodyViewModel.Setup(x => x.MultipleFiniteStateSelector).Returns(mockMultipleFiniteStateSelector.Object);
            mockViewerBodyViewModel.Setup(x => x.ProductTreeViewModel).Returns(new ViewerProductTreeViewModel(selectionMediator.Object));
            mockViewerBodyViewModel.Setup(x => x.CanvasViewModel).Returns(this.canvasViewModel.Object);
            mockViewerBodyViewModel.Setup(x => x.PropertiesViewModel).Returns(mockPropertiesViewModel.Object);

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
        /// Verifies that <see cref="ViewerBody" /> re-initializes canvas when <see cref="ViewerBody.OnSplitViewValueChanged" /> emits false.
        /// </summary>
        [Test]
        public void VerifyOnSplitViewValueChangedHandler()
        {
            var subject = new Subject<bool>();

            this.context.Render<ViewerBody>(parameters =>
            {
                parameters.AddCascadingValue(WebAppConstantValues.OnSplitViewValueChangedCascadingValueName, subject);
            });

            // Initial component render triggers InitCanvas(true) once via OnAfterRenderAsync
            this.canvasViewModel.Verify(x => x.InitCanvas(true), Times.Once);

            // Emitting true (split view enabled) should NOT re-init canvas again
            subject.OnNext(true);
            this.canvasViewModel.Verify(x => x.InitCanvas(true), Times.Once);

            // Emitting false (split view disabled) SHOULD re-init canvas a second time
            subject.OnNext(false);
            this.canvasViewModel.Verify(x => x.InitCanvas(true), Times.Exactly(2));
        }
    }
}
