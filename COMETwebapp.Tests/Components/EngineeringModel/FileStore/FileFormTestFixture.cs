// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FileFormTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.EngineeringModel.FileStore
{
    using Bunit;

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Components.EngineeringModel.FileStore;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileHandler;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components.Forms;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class FileFormTestFixture
    {
        private TestContext context;
        private IRenderedComponent<FileForm> renderer;
        private Mock<IFileHandlerViewModel> viewModel;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.viewModel = new Mock<IFileHandlerViewModel>();
            this.viewModel.Setup(x => x.File).Returns(new File());
            var domainSelectorViewModel = new Mock<IDomainOfExpertiseSelectorViewModel>();
            this.viewModel.Setup(x => x.DomainOfExpertiseSelectorViewModel).Returns(domainSelectorViewModel.Object);
            this.context.ConfigureDevExpressBlazor();

            this.renderer = this.context.RenderComponent<FileForm>(parameters => { parameters.Add(p => p.ViewModel, this.viewModel.Object); });
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.context.Dispose();
        }

        [Test]
        public async Task VerifyOnValidSubmitAndDelete()
        {
            Assert.That(this.renderer.Instance.IsDeletePopupVisible, Is.EqualTo(false));

            var form = this.renderer.FindComponent<EditForm>();
            await this.renderer.InvokeAsync(form.Instance.OnValidSubmit.InvokeAsync);
            this.viewModel.Verify(x => x.CreateOrEditFile(It.IsAny<bool>()), Times.Once);

            var deleteFileButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "deleteFileButton");
            await this.renderer.InvokeAsync(deleteFileButton.Instance.Click.InvokeAsync);
            Assert.That(this.renderer.Instance.IsDeletePopupVisible, Is.EqualTo(true));

            var deleteFilePopupButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "deleteFilePopupButton");
            await this.renderer.InvokeAsync(deleteFilePopupButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.IsDeletePopupVisible, Is.EqualTo(false));
                this.viewModel.Verify(x => x.DeleteFile(), Times.Once);
            });
        }
    }
}
