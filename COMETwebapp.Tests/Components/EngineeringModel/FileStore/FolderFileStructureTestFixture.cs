// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FolderFileStructureTestFixture.cs" company="Starion Group S.A.">
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
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileHandler;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FolderHandler;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components.Web;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class FolderFileStructureTestFixture
    {
        private TestContext context;
        private IRenderedComponent<FolderFileStructure> renderer;
        private Mock<IFolderFileStructureViewModel> viewModel;
        private Mock<IFileHandlerViewModel> fileHandlerViewModel;
        private Mock<IFolderHandlerViewModel> folderHandlerViewModel;
        private List<FileFolderNodeViewModel> structure;

        [SetUp]
         public void SetUp()
         {
             this.context = new TestContext();
             this.viewModel = new Mock<IFolderFileStructureViewModel>();
             this.folderHandlerViewModel = new Mock<IFolderHandlerViewModel>();
             this.fileHandlerViewModel = new Mock<IFileHandlerViewModel>();

             var file = new File();

             file.FileRevision.Add(new FileRevision()
             {
                 Name = "File Revision 1"
             });

             this.structure =
             [
                 new FileFolderNodeViewModel()
                 {
                     Content =
                     {
                         new FileFolderNodeViewModel(file),
                         new FileFolderNodeViewModel(new Folder(), [new FileFolderNodeViewModel(file)])
                     }
                 }
             ];

             this.fileHandlerViewModel.Setup(x => x.File).Returns(new File());
             this.folderHandlerViewModel.Setup(x => x.Folder).Returns(new Folder());

             this.viewModel.Setup(x => x.Structure).Returns(this.structure);
             this.viewModel.Setup(x => x.FileHandlerViewModel).Returns(this.fileHandlerViewModel.Object);
             this.viewModel.Setup(x => x.FolderHandlerViewModel).Returns(this.folderHandlerViewModel.Object);

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
            var rootNode = this.structure[0];

            treeNodeEventMock.Setup(x => x.DataItem).Returns(rootNode.Content.First(x => x.Thing is not File));
            this.renderer.Instance.OnNodeClick(treeNodeEventMock.Object);
            Assert.That(this.renderer.Instance.IsFileFormVisibile, Is.EqualTo(false));

            treeNodeEventMock.Setup(x => x.DataItem).Returns(rootNode.Content.First(x => x.Thing is File));
            this.renderer.Instance.OnNodeClick(treeNodeEventMock.Object);
            this.renderer.Render();

            var form = this.renderer.FindComponent<FileForm>();

            Assert.Multiple(() =>
            {
                Assert.That(form, Is.Not.Null);
                Assert.That(form.Instance.ShouldCreate, Is.EqualTo(false));
                Assert.That(this.renderer.Instance.IsFileFormVisibile, Is.EqualTo(true));
                Assert.That(this.renderer.Instance.IsFolderFormVisibile, Is.EqualTo(false));
            });

            var popup = this.renderer.FindComponent<DxPopup>();
            await this.renderer.InvokeAsync(popup.Instance.Closed.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.IsFileFormVisibile, Is.EqualTo(false));
                Assert.That(this.renderer.Instance.IsFolderFormVisibile, Is.EqualTo(false));
            });
         }

        [Test]
        public void VerifyEditFolder()
        {
            var rootNode = this.structure[0];
            var notFolderRow = rootNode.Content.First(x => x.Thing is not Folder);
            this.renderer.Instance.OnEditFolderClick(notFolderRow);
            Assert.That(this.renderer.Instance.IsFolderFormVisibile, Is.EqualTo(false));

            var folderRow = rootNode.Content.First(x => x.Thing is Folder);
            this.renderer.Instance.OnEditFolderClick(folderRow);

            this.renderer.Render();

            var form = this.renderer.FindComponent<FolderForm>();

            Assert.Multiple(() =>
            {
                Assert.That(form, Is.Not.Null);
                Assert.That(form.Instance.ShouldCreate, Is.EqualTo(false));
                Assert.That(this.renderer.Instance.IsFileFormVisibile, Is.EqualTo(false));
                Assert.That(this.renderer.Instance.IsFolderFormVisibile, Is.EqualTo(true));
            });
        }

        [Test]
        public async Task VerifyDragAndDrop()
        {
            var rootNode = this.structure.First();

            // Test drag a file into a folder
            var draggedFileNode = rootNode.Content[0];
            var draggableFile = this.renderer.FindAll("div").Where(x => x.Attributes["draggable"]?.Value == "true").ElementAt(1);
            await this.renderer.InvokeAsync(() => draggableFile.DragStartAsync(new DragEventArgs()));
            Assert.That(this.renderer.Instance.DraggedNode, Is.EqualTo(draggedFileNode));

            var droppableDiv = this.renderer.FindAll("div").Where(x => x.Attributes["draggable"]?.Value == "true").ElementAt(0);
            await this.renderer.InvokeAsync(() => droppableDiv.DropAsync(new DragEventArgs()));
            this.fileHandlerViewModel.Verify(x => x.MoveFile((File)draggedFileNode.Thing, (Folder)rootNode.Thing));

            // Test drag a folder into a folder
            var draggedFolderNode = rootNode.Content[1];
            var draggableFolder = this.renderer.FindAll("div").Where(x => x.Attributes["draggable"]?.Value == "true").ElementAt(2);
            await this.renderer.InvokeAsync(() => draggableFolder.DragStartAsync(new DragEventArgs()));
            Assert.That(this.renderer.Instance.DraggedNode, Is.EqualTo(draggedFolderNode));

            droppableDiv = this.renderer.FindAll("div").Where(x => x.Attributes["draggable"]?.Value == "true").ElementAt(0);
            await this.renderer.InvokeAsync(() => droppableDiv.DropAsync(new DragEventArgs()));
            this.folderHandlerViewModel.Verify(x => x.MoveFolder((Folder)draggedFolderNode.Thing, (Folder)rootNode.Thing));

            // Test drop something in a file - invalid
            var invalidDroppableDiv = this.renderer.FindAll("div").Where(x => x.Attributes["draggable"]?.Value == "true").ElementAt(1);
            await this.renderer.InvokeAsync(() => invalidDroppableDiv.DropAsync(new DragEventArgs()));
            this.fileHandlerViewModel.Verify(x => x.MoveFile((File)draggedFileNode.Thing, (Folder)rootNode.Thing));
        }
    }
}
