// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FileTypeFormTestFixture.cs" company="Starion Group S.A.">
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

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Common;
    using COMETwebapp.Components.EngineeringModel.FileStore;

    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class FileTypeFormTestFixture
    {
        private BunitContext context;
        private IRenderedComponent<FileTypeForm> renderer;
        private FileType fileType;
        private Mock<ISessionService> sessionService;
        private bool isSaved;
        private bool isCanceled;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.sessionService = new Mock<ISessionService>();
            this.context.Services.AddSingleton(this.sessionService.Object);

            this.fileType = new FileType { Name = "application/pdf", Extension = "pdf" };
            this.isSaved = false;
            this.isCanceled = false;

            var availableFileTypes = new List<FileType>
            {
                this.fileType,
                new() { Name = "application/json", Extension = "json" }
            };

            this.renderer = this.context.Render<FileTypeForm>(parameters => parameters
                .Add(p => p.FileType, this.fileType)
                .Add(p => p.FileTypeChanged, ft => this.fileType = ft)
                .Add(p => p.AvailableFileTypes, availableFileTypes)
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
        public async Task VerifyFileTypeFormSubmissionAndCancellation()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.renderer.Instance.FileType, Is.SameAs(this.fileType));
                Assert.That(this.renderer.Instance.AvailableFileTypes.ToList(), Has.Count.EqualTo(2));
            }

            var newFileType = this.renderer.Instance.AvailableFileTypes.Last();
            await this.renderer.InvokeAsync(() => this.renderer.Instance.OnFileTypeChanged(newFileType));

            Assert.That(this.fileType, Is.SameAs(newFileType));

            var editForm = this.renderer.FindComponent<EditForm>();
            await this.renderer.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);
            Assert.That(this.isSaved, Is.True);

            var formButtons = this.renderer.FindComponent<FormButtons>();
            await this.renderer.InvokeAsync(formButtons.Instance.OnCancel.InvokeAsync);
            Assert.That(this.isCanceled, Is.True);
        }
    }
}
