// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FileDropZoneTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.Components
{
    using Bunit;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Test.Helpers;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="FileDropZone" /> component.
    /// </summary>
    [TestFixture]
    public class FileDropZoneTestFixture
    {
        private BunitContext context;

        [SetUp]
        public void Setup()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyPlaceholderIsRenderedWhenNoFileIsSelected()
        {
            var renderer = this.context.Render<FileDropZone>(parameters => parameters
                .Add(p => p.Id, "upload-zone")
                .Add(p => p.Accept, ".zip")
                .Add(p => p.Placeholder, "Drop a file here"));

            var inputFile = renderer.FindComponent<InputFile>();

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Find("#upload-zone"), Is.Not.Null);
                Assert.That(renderer.Markup, Does.Contain("Drop a file here"));
                Assert.That(inputFile.Find("input").Id, Is.EqualTo("upload-zone-input"));
                Assert.That(inputFile.Find("input").GetAttribute("accept"), Is.EqualTo(".zip"));
                Assert.That(renderer.FindAll("#upload-zone-selected-file"), Is.Empty);
            });
        }

        [Test]
        public void VerifySelectedFileNameIsRendered()
        {
            var fileMock = new Mock<IBrowserFile>();
            fileMock.Setup(x => x.Name).Returns("archive.zip");

            var renderer = this.context.Render<FileDropZone>(parameters => parameters
                .Add(p => p.Id, "upload-zone")
                .Add(p => p.SelectedFile, fileMock.Object));

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Find("#upload-zone-selected-file").TextContent, Is.EqualTo("archive.zip"));
                Assert.That(renderer.Markup, Does.Not.Contain("Drop a file here, or click to browse"));
            });
        }

        [Test]
        public async Task VerifySelectedFileChangedIsRaisedWhenFileIsPicked()
        {
            var fileMock = new Mock<IBrowserFile>();
            fileMock.Setup(x => x.Name).Returns("archive.zip");

            IBrowserFile capturedFile = null;

            var renderer = this.context.Render<FileDropZone>(parameters => parameters
                .Add(p => p.Id, "upload-zone")
                .Add(p => p.SelectedFileChanged, EventCallback.Factory.Create<IBrowserFile>(this, file => capturedFile = file)));

            var fileInput = renderer.FindComponent<InputFile>();

            await renderer.InvokeAsync(() => fileInput.Instance.OnChange.InvokeAsync(new InputFileChangeEventArgs([fileMock.Object])));

            Assert.That(capturedFile, Is.SameAs(fileMock.Object));
        }

        [Test]
        public void VerifyDefaultId()
        {
            var renderer = this.context.Render<FileDropZone>();

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Find("#file-drop-zone"), Is.Not.Null);
                Assert.That(renderer.FindComponent<InputFile>().Find("input").Id, Is.EqualTo("file-drop-zone-input"));
            });
        }
    }
}
