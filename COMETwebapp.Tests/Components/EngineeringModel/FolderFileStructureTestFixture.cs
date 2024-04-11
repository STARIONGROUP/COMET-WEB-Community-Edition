// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FolderFileStructureTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Components.EngineeringModel
{
    using System.Linq;
    using System.Threading.Tasks;

    using Bunit;

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.EngineeringModel;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FolderFileStructure;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components.Forms;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class FolderFileStructureTestFixture
    {
        private TestContext context;
        private IRenderedComponent<FolderFileStructure> renderer;
        private Mock<IFolderFileStructureViewModel> viewModel;
        private List<FileFolderNodeViewModel> structure;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.viewModel = new Mock<IFolderFileStructureViewModel>();

            var file = new File();

            file.FileRevision.Add(new FileRevision()
            {
                Name = "File Revision 1"
            });

            this.structure = 
            [
                new FileFolderNodeViewModel(file),
                new FileFolderNodeViewModel(new Folder(), [new FileFolderNodeViewModel(file)]),
            ];

            this.viewModel.Setup(x => x.File).Returns(new File());
            this.viewModel.Setup(x => x.Folder).Returns(new Folder());
            this.viewModel.Setup(x => x.Structure).Returns(this.structure);
            
            this.context.ConfigureDevExpressBlazor();

            this.renderer = this.context.RenderComponent<FolderFileStructure>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel.Object);
            });
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.context.Dispose();
        }

        [Test]
        public void VerifyOnInitialized()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ViewModel, Is.EqualTo(this.viewModel.Object));
                Assert.That(this.renderer.Markup, Does.Contain(this.structure.First().Name));
            });
        }

        [Test]
        public async Task VerifyEditFile()
        {
            var treeNodeEventMock = new Mock<ITreeViewNodeInfo>();
            treeNodeEventMock.Setup(x => x.DataItem).Returns(this.structure.First(x => x.Thing is not File));
            this.renderer.Instance.OnNodeClick(treeNodeEventMock.Object);
            Assert.That(this.renderer.Instance.SelectedFile, Is.Null);

            treeNodeEventMock.Setup(x => x.DataItem).Returns(this.structure.First(x => x.Thing is File));
            this.renderer.Instance.OnNodeClick(treeNodeEventMock.Object);
            this.renderer.Render();

            var form = this.renderer.FindComponent<FileForm>();

            Assert.Multiple(() =>
            {
                Assert.That(form, Is.Not.Null);
                Assert.That(form.Instance.ShouldCreate, Is.EqualTo(false));
                Assert.That(this.renderer.Instance.SelectedFile, Is.Not.Null);
                Assert.That(this.renderer.Instance.SelectedFolder, Is.Null);
            });

            // Test behavior in case the submmit is valid => next step
            var editForm = form.FindComponent<EditForm>();
            await form.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);

            var cancelButton = editForm.FindComponents<DxButton>().First(x => x.Instance.Id == "cancelFileButton");
            await editForm.InvokeAsync(cancelButton.Instance.Click.InvokeAsync);
            Assert.That(this.renderer.Instance.SelectedFile, Is.Null);
        }

        [Test]
        public async Task VerifyEditFolder()
        {
            var notFolderRow = this.structure.First(x => x.Thing is not Folder);
            this.renderer.Instance.OnEditFolderClick(notFolderRow);
            Assert.That(this.renderer.Instance.SelectedFolder, Is.Null);

            var folderRow = this.structure.First(x => x.Thing is Folder);
            this.renderer.Instance.OnEditFolderClick(folderRow);

            this.renderer.Render();

            var form = this.renderer.FindComponent<FolderForm>();

            Assert.Multiple(() =>
            {
                Assert.That(form, Is.Not.Null);
                Assert.That(form.Instance.ShouldCreate, Is.EqualTo(false));
                Assert.That(this.renderer.Instance.SelectedFile, Is.Null);
                Assert.That(this.renderer.Instance.SelectedFolder, Is.Not.Null);
            });

            // Test behavior in case the submmit is valid => next step
            var editForm = form.FindComponent<EditForm>();
            await form.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);

            var cancelButton = editForm.FindComponents<DxButton>().First(x => x.Instance.Id == "cancelFolderButton");
            await editForm.InvokeAsync(cancelButton.Instance.Click.InvokeAsync);
            Assert.That(this.renderer.Instance.SelectedFolder, Is.Null);
        }
    }
}
