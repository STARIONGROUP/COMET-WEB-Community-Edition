// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileRevisionsTableTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Components.EngineeringModel.FileStore
{
    using System.Linq;

    using Bunit;

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.EngineeringModel.FileStore;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileRevisionHandler;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components.Forms;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class FileRevisionsTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<FileRevisionsTable> renderer;
        private Mock<IFileRevisionHandlerViewModel> viewModel;
        private List<FileRevision> fileRevisions;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.viewModel = new Mock<IFileRevisionHandlerViewModel>();
            this.fileRevisions = [new FileRevision()];

            this.viewModel.Setup(x => x.FileRevision).Returns(new FileRevision());
            this.viewModel.Setup(x => x.CurrentFile).Returns(new File());
            this.context.ConfigureDevExpressBlazor();

            this.renderer = this.context.RenderComponent<FileRevisionsTable>(parameters =>
            { 
                parameters.Add(p => p.ViewModel, this.viewModel.Object);
                parameters.Add(p => p.FileRevisions, this.fileRevisions);
            });
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.context.Dispose();
        }

        [Test]
        public async Task VerifyRowActions()
        { 
            var timesFileRevisionsWasChanged = 0;

            this.renderer.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.FileRevisionsChanged, () => { timesFileRevisionsWasChanged += 1; });
            });

            var downloadButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "downloadFileRevisionButton");
            await this.renderer.InvokeAsync(downloadButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.DownloadFileRevision(It.IsAny<FileRevision>()), Times.Once);

            var removeFileRevisionButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "removeFileRevisionButton");
            await this.renderer.InvokeAsync(removeFileRevisionButton.Instance.Click.InvokeAsync);
            Assert.That(timesFileRevisionsWasChanged, Is.EqualTo(1));
        }

        [Test]
        public async Task VerifyFileRevisionCreation()
        {
            var timesFileRevisionsWasChanged = 0;

            this.renderer.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.FileRevisionsChanged, () => { timesFileRevisionsWasChanged += 1; });
            });

            var addFileRevisionButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addFileRevisionButton");
            await this.renderer.InvokeAsync(addFileRevisionButton.Instance.Click.InvokeAsync);

            var fileInput = this.renderer.FindComponent<InputFile>();
            var fileMock = new Mock<IBrowserFile>();
            var changeArgs = new InputFileChangeEventArgs([fileMock.Object]);
            await this.renderer.InvokeAsync(() => fileInput.Instance.OnChange.InvokeAsync(changeArgs));
            this.viewModel.Verify(x => x.UploadFile(fileMock.Object), Times.Once);

            var grid = this.renderer.FindComponent<DxGrid>();
            await this.renderer.InvokeAsync(grid.Instance.EditModelSaving.InvokeAsync);
            Assert.That(timesFileRevisionsWasChanged, Is.EqualTo(1));
        }
    }
}
