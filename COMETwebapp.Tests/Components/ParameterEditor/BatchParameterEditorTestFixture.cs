// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BatchParameterEditorTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.Tests.Components.ParameterEditor
{
    using Bunit;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Components.Selectors;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Components.ParameterEditor.BatchParameterEditor;
    using COMETwebapp.ViewModels.Components.ParameterEditor.BatchParameterEditor;

    using DevExpress.Blazor;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class BatchParameterEditorTestFixture
    {
        private TestContext context;
        private IRenderedComponent<BatchParameterEditor> renderer;
        private Mock<IBatchParameterEditorViewModel> viewModel;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            var parameterTypeSelectorViewModel = new Mock<IParameterTypeSelectorViewModel>();
            var parameterTypeEditorSelectorViewModel = new Mock<IParameterTypeEditorSelectorViewModel>();
            var confirmCancelPopupViewModel = new Mock<IConfirmCancelPopupViewModel>();

            this.viewModel = new Mock<IBatchParameterEditorViewModel>();

            this.viewModel.Setup(x => x.IsVisible).Returns(true);
            this.viewModel.Setup(x => x.ConfirmCancelPopupViewModel).Returns(confirmCancelPopupViewModel.Object);
            this.viewModel.Setup(x => x.ParameterTypeSelectorViewModel).Returns(parameterTypeSelectorViewModel.Object);
            this.viewModel.Setup(x => x.ParameterTypeEditorSelectorViewModel).Returns(parameterTypeEditorSelectorViewModel.Object);

            this.renderer = this.context.RenderComponent<BatchParameterEditor>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel.Object);
            });
        }

        [TearDown]
        public void TearDown()
        {
            this.context.Dispose();
            this.context.CleanContext();
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer, Is.Not.Null);
                Assert.That(this.viewModel, Is.Not.Null);
                Assert.That(this.renderer.Instance, Is.Not.Null);
            });
        }

        [Test]
        public async Task VerifyComponentInteractions()
        {
            var openPopupButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "openBatchParameterEditorButton");
            await this.renderer.InvokeAsync(openPopupButton.Instance.Click.InvokeAsync);

            var parameterTypeSelector = this.renderer.FindComponent<ParameterTypeSelector>();

            Assert.Multiple(() =>
            {
                Assert.That(parameterTypeSelector.Instance, Is.Not.Null);
                Assert.That(parameterTypeSelector.Instance.ViewModel, Is.EqualTo(this.viewModel.Object.ParameterTypeSelectorViewModel));
            });

            var confirmCancelPopup = this.renderer.FindComponent<ConfirmCancelPopup>();
            await this.renderer.InvokeAsync(confirmCancelPopup.Instance.ViewModel.OnConfirm.InvokeAsync);
            this.viewModel.Verify(x => x.IsVisible);
        }
    }
}
