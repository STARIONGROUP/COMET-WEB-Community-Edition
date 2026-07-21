// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MatrixConfigurationDialogTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.RelationshipMatrix
{
    using Bunit;

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Services.FileStore;
    using COMETwebapp.ViewModels.Components.RelationshipMatrix;

    using Microsoft.AspNetCore.Components.Web;

    using Moq;

    using NUnit.Framework;

    using DialogComponent = COMETwebapp.Components.RelationshipMatrix.MatrixConfigurationDialog;

    [TestFixture]
    public class MatrixConfigurationDialogTestFixture
    {
        private BunitContext context;
        private Mock<IRelationshipMatrixBodyViewModel> viewModel;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.viewModel = new Mock<IRelationshipMatrixBodyViewModel>();
            this.viewModel.Setup(x => x.IsConfigurationDialogVisible).Returns(true);
            this.viewModel.Setup(x => x.FileStoreMessage).Returns(string.Empty);
            this.viewModel.Setup(x => x.CanUseModelFileStore).Returns(true);
            this.viewModel.Setup(x => x.StoreExists(It.IsAny<FileStoreType>())).Returns(true);
            this.viewModel.Setup(x => x.GetFolders(It.IsAny<FileStoreType>())).Returns([]);
            this.viewModel.Setup(x => x.GetStoredConfigurations(It.IsAny<FileStoreType>())).Returns([]);
            this.viewModel.Setup(x => x.ExportConfigurationAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            this.viewModel.Setup(x => x.CreateFileStoreAsync(It.IsAny<FileStoreType>())).ReturnsAsync(true);
            this.viewModel.Setup(x => x.SaveConfigurationToStoreAsync(It.IsAny<FileStoreType>(), It.IsAny<string>(), It.IsAny<Folder>())).ReturnsAsync(true);
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        private IRenderedComponent<DialogComponent> RenderDialog()
        {
            return this.context.Render<DialogComponent>(parameters => parameters.Add(p => p.ViewModel, this.viewModel.Object));
        }

        [Test]
        public void VerifyDialogRendersBothSections()
        {
            var renderedComponent = this.RenderDialog();

            Assert.Multiple(() =>
            {
                Assert.That(renderedComponent.Markup, Does.Contain("Export configuration"));
                Assert.That(renderedComponent.Markup, Does.Contain("Import configuration"));
                Assert.That(renderedComponent.Markup, Does.Contain("Download"));
                Assert.That(renderedComponent.Find("#importMatrixConfig"), Is.Not.Null);
            });
        }

        [Test]
        public async Task VerifyExportInvokesViewModel()
        {
            var renderedComponent = this.RenderDialog();

            var downloadButton = renderedComponent.FindAll("button").Single(x => x.TextContent.Contains("Download"));
            await renderedComponent.InvokeAsync(() => downloadButton.ClickAsync(new MouseEventArgs()));

            this.viewModel.Verify(x => x.ExportConfigurationAsync(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void VerifyModelStoreHintShownWithoutJsonFileType()
        {
            this.viewModel.Setup(x => x.CanUseModelFileStore).Returns(false);

            var renderedComponent = this.RenderDialog();

            Assert.That(renderedComponent.Markup, Does.Contain("Add a JSON file type to the RDL"));
        }
    }
}
