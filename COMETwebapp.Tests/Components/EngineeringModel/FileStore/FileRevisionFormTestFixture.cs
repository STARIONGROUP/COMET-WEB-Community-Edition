// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FileRevisionFormTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.EngineeringModel.FileStore
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Common;
    using COMETwebapp.Components.EngineeringModel.FileStore;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileRevisionHandler;

    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class FileRevisionFormTestFixture
    {
        private BunitContext context;
        private IRenderedComponent<FileRevisionForm> renderer;
        private Mock<IFileRevisionHandlerViewModel> viewModel;
        private Mock<ISessionService> sessionService;
        private FileRevision fileRevision;
        private bool isSaved;
        private bool isCanceled;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.sessionService = new Mock<ISessionService>();
            this.context.Services.AddSingleton(this.sessionService.Object);

            this.fileRevision = new FileRevision
            {
                Name = "Revision 1"
            };

            this.viewModel = new Mock<IFileRevisionHandlerViewModel>();
            this.viewModel.Setup(x => x.FileRevision).Returns(this.fileRevision);
            this.viewModel.Setup(x => x.FileTypes).Returns([new FileType { Name = "text/plain", Extension = "txt" }]);
            this.viewModel.Setup(x => x.ErrorMessage).Returns(string.Empty);

            this.isSaved = false;
            this.isCanceled = false;

            this.renderer = this.context.Render<FileRevisionForm>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.OnSaved, () => Task.FromResult(this.isSaved = true))
                .Add(p => p.OnCanceled, () => Task.FromResult(this.isCanceled = true)));
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.context.Dispose();
        }

        [Test]
        public async Task VerifyFileRevisionFormSubmissionAndFileUpload()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.renderer.Instance.ViewModel, Is.SameAs(this.viewModel.Object));
                Assert.That(this.renderer.Markup, Does.Contain("Revision 1"));
            }

            this.viewModel.Setup(x => x.ErrorMessage).Returns("Error uploading file");
            this.renderer.Render();
            Assert.That(this.renderer.Markup, Does.Contain("Error uploading file"));

            var fileInput = this.renderer.FindComponent<InputFile>();
            var fileMock = new Mock<IBrowserFile>();
            var changeArgs = new InputFileChangeEventArgs([fileMock.Object]);
            await this.renderer.InvokeAsync(() => fileInput.Instance.OnChange.InvokeAsync(changeArgs));
            this.viewModel.Verify(x => x.UploadFile(fileMock.Object), Times.Once);

            var editForm = this.renderer.FindComponent<EditForm>();
            await this.renderer.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);
            Assert.That(this.isSaved, Is.True);

            var formButtons = this.renderer.FindComponent<FormButtons>();
            Assert.That(formButtons.Instance.SaveButtonEnabled, Is.True);

            await this.renderer.InvokeAsync(formButtons.Instance.OnCancel.InvokeAsync);
            Assert.That(this.isCanceled, Is.True);
        }
    }
}
