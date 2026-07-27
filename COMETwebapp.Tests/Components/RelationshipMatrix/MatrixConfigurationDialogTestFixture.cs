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
    using System.IO;
    using System.Reflection;

    using Bunit;

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Model.RelationshipMatrix;
    using COMETwebapp.Services.FileStore;
    using COMETwebapp.ViewModels.Components.RelationshipMatrix;

    using Microsoft.AspNetCore.Components.Forms;
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

        [Test]
        public async Task VerifyImportFromFileInvokesViewModel()
        {
            this.viewModel.Setup(x => x.ImportConfigurationAsync(It.IsAny<Stream>())).Returns(Task.CompletedTask);

            var renderedComponent = this.RenderDialog();

            var file = InputFileContent.CreateFromText("{}", "config.json");
            await renderedComponent.InvokeAsync(() => renderedComponent.FindComponent<InputFile>().UploadFiles(file));

            this.viewModel.Verify(x => x.ImportConfigurationAsync(It.IsAny<Stream>()), Times.Once);
            this.viewModel.VerifySet(x => x.IsConfigurationDialogVisible = false);
        }

        [Test]
        public async Task VerifySaveToModelCreatesStoreAndCloses()
        {
            this.viewModel.Setup(x => x.StoreExists(FileStoreType.Domain)).Returns(false);
            this.viewModel.Setup(x => x.CreateFileStoreAsync(FileStoreType.Domain)).ReturnsAsync(true);

            var renderedComponent = this.RenderDialog();

            // The destination combo is a DevExpress component that cannot be driven in bunit, so the selection is set on
            // the component directly to reach the model-store save branch (verified end-to-end elsewhere).
            SetPrivateMember(renderedComponent.Instance, "Destination", ConfigurationDestination.DomainFileStore);
            renderedComponent.Render();

            Assert.That(renderedComponent.Markup, Does.Contain("will be created"));

            var saveButton = renderedComponent.FindAll("button").Single(x => x.TextContent.Contains("Save to model"));
            await renderedComponent.InvokeAsync(() => saveButton.ClickAsync(new MouseEventArgs()));

            Assert.Multiple(() =>
            {
                this.viewModel.Verify(x => x.CreateFileStoreAsync(FileStoreType.Domain), Times.Once);
                this.viewModel.Verify(x => x.SaveConfigurationToStoreAsync(FileStoreType.Domain, It.IsAny<string>(), It.IsAny<Folder>()), Times.Once);
                this.viewModel.VerifySet(x => x.IsConfigurationDialogVisible = false);
            });
        }

        [Test]
        public async Task VerifyLoadFromModelInvokesViewModelAndCloses()
        {
            var storedConfiguration = new CDP4Common.EngineeringModelData.File();

            var renderedComponent = this.RenderDialog();

            SetPrivateMember(renderedComponent.Instance, "ImportStore", FileStoreType.Common);
            SetPrivateMember(renderedComponent.Instance, "selectedStoredConfiguration", storedConfiguration);
            renderedComponent.Render();

            var loadButton = renderedComponent.FindAll("button").Single(x => x.TextContent.Contains("Load from model"));
            await renderedComponent.InvokeAsync(() => loadButton.ClickAsync(new MouseEventArgs()));

            this.viewModel.Verify(x => x.LoadConfigurationFromStoreAsync(storedConfiguration), Times.Once);
            this.viewModel.VerifySet(x => x.IsConfigurationDialogVisible = false);
        }

        /// <summary>
        /// Sets a non-public property (via its setter) or field on the component, so selections normally made through a
        /// DevExpress combo (which bunit cannot drive) can be reached in a component test.
        /// </summary>
        private static void SetPrivateMember(object target, string name, object value)
        {
            var property = target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic);

            if (property != null)
            {
                property.SetValue(target, value);
                return;
            }

            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }
    }
}
