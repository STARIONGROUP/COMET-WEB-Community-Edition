// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileTypesTableTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.EngineeringModel.FileStore;

    using DevExpress.Blazor;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class FileTypesTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<FileTypesTable> renderer;
        private FileRevision fileRevision;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.fileRevision = new FileRevision();

            var pdfFileType = new FileType()
            {
                Name = "application/pdf",
                ShortName = "application/pdf",
                Extension = "pdf"
            };

            var jsonFileType = new FileType()
            {
                Name = "application/json",
                ShortName = "application/json",
                Extension = "json"
            };

            var textFileType = new FileType()
            {
                Name = "application/txt",
                ShortName = "application/txt",
                Extension = "txt"
            };

            var siteRdl = new SiteReferenceDataLibrary()
            {
                FileType = { pdfFileType, jsonFileType, textFileType }
            };

            this.fileRevision.FileType.AddRange([pdfFileType, jsonFileType]);
            this.context.ConfigureDevExpressBlazor();

            this.renderer = this.context.RenderComponent<FileTypesTable>(parameters =>
            { 
                parameters.Add(p => p.FileTypes, siteRdl.FileType);
                parameters.Add(p => p.SelectedFileTypes, this.fileRevision.FileType);
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
            var timesSelectedFileTypesWasChanged = 0;

            this.renderer.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.SelectedFileTypesChanged, () => { timesSelectedFileTypesWasChanged += 1; });
            });

            Assert.Multiple(() =>
            {
                Assert.That(this.fileRevision.FileType[0].Extension, Is.EqualTo("pdf"));
                Assert.That(this.fileRevision.FileType[1].Extension, Is.EqualTo("json"));
                Assert.That(timesSelectedFileTypesWasChanged, Is.EqualTo(0));
            });
            
            var moveUpButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "moveUpButton" && x.Instance.Enabled);
            await this.renderer.InvokeAsync(moveUpButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.fileRevision.FileType[0].Extension, Is.EqualTo("json"));
                Assert.That(this.fileRevision.FileType[1].Extension, Is.EqualTo("pdf"));
                Assert.That(timesSelectedFileTypesWasChanged, Is.EqualTo(1));
            });

            var moveDownButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "moveDownButton" && x.Instance.Enabled);
            await this.renderer.InvokeAsync(moveDownButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.fileRevision.FileType[0].Extension, Is.EqualTo("pdf"));
                Assert.That(this.fileRevision.FileType[1].Extension, Is.EqualTo("json"));
                Assert.That(timesSelectedFileTypesWasChanged, Is.EqualTo(2));
            });

            var removeFileTypeButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "removeFileTypeButton");
            await this.renderer.InvokeAsync(removeFileTypeButton.Instance.Click.InvokeAsync);
            Assert.That(timesSelectedFileTypesWasChanged, Is.EqualTo(3));
        }
        
        [Test]
        public async Task VerifyFileTypeCreation()
        {
            var timesSelectedFileTypesWasChanged = 0;

            this.renderer.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.SelectedFileTypesChanged, () => { timesSelectedFileTypesWasChanged += 1; });
            });

            var addFileTypeButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addFileTypeButton");
            await this.renderer.InvokeAsync(addFileTypeButton.Instance.Click.InvokeAsync);

            var grid = this.renderer.FindComponent<DxGrid>();
            await this.renderer.InvokeAsync(grid.Instance.EditModelSaving.InvokeAsync);
            Assert.That(timesSelectedFileTypesWasChanged, Is.EqualTo(1));
        }
    }
}
