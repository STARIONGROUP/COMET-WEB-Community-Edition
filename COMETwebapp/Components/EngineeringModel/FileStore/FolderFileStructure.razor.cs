// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FolderFileStructure.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.Components.EngineeringModel.FileStore
{
    using System.ComponentModel.DataAnnotations;

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Extensions;

    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Support class for the <see cref="FolderFileStructure"/>
    /// </summary>
    public partial class FolderFileStructure
    {
        /// <summary>
        /// The <see cref="IFolderFileStructureViewModel" /> for this component
        /// </summary>
        [Parameter, Required]
        public IFolderFileStructureViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets the condition to verify if a file should be created
        /// </summary>
        public bool ShouldCreateFile { get; private set; }

        /// <summary>
        /// Gets the condition to check the visibility of the file form
        /// </summary>
        public bool IsFileFormVisibile { get; private set; }

        /// <summary>
        /// Gets the condition to verify if a folder should be created
        /// </summary>
        public bool ShouldCreateFolder { get; private set; }

        /// <summary>
        /// Gets the condition to check the visibility of the folder form
        /// </summary>
        public bool IsFolderFormVisibile { get; private set; }

        /// <summary>
        /// Gets the dragged node used in drag and drop interactions
        /// </summary>
        public FileFolderNodeViewModel DraggedNode { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="DxTreeView"/> used to display the folder-file structure
        /// </summary>
        private DxTreeView TreeView { get; set; }

        /// <summary>
        /// Method that is triggered every time a node is clicked
        /// </summary>
        /// <param name="e">The <see cref="TreeViewNodeClickEventArgs"/></param>
        public void OnNodeClick(ITreeViewNodeInfo e)
        {
            var dataItem = (FileFolderNodeViewModel)e.DataItem;
            this.OnEditFileClick(dataItem);
        }

        /// <summary>
        /// Method that is triggered every time the edit folder is clicked
        /// </summary>
        /// <param name="row">The selected row</param>
        public void OnEditFolderClick(FileFolderNodeViewModel row = null)
        {
            if (row is { Thing: not Folder })
            {
                return;
            }

            this.ViewModel.FolderHandlerViewModel.SelectFolder(row == null ? new Folder() : (Folder)row.Thing);
            this.ShouldCreateFolder = row == null;
            this.IsFolderFormVisibile = true;
        }

        /// <summary>
        /// Method that is triggered every time the edit file is clicked
        /// </summary>
        /// <param name="row">The selected row</param>
        public void OnEditFileClick(FileFolderNodeViewModel row = null)
        {
            if (row is { Thing: not File })
            {
                return;
            }

            this.ViewModel.FileHandlerViewModel.SelectFile(row == null ? new File() : (File)row.Thing);
            this.ShouldCreateFile = row == null;
            this.IsFileFormVisibile = true;
        }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsLoading).SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Method invoked after each time the component has rendered interactively and the UI has finished
        /// updating (for example, after elements have been added to the browser DOM). Any <see cref="T:Microsoft.AspNetCore.Components.ElementReference" />
        /// fields will be populated by the time this runs.
        /// This method is not invoked during prerendering or server-side rendering, because those processes
        /// are not attached to any live browser DOM and are already complete before the DOM is updated.
        /// </summary>
        /// <param name="firstRender">
        /// Set to <c>true</c> if this is the first time <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> has been invoked
        /// on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <remarks>
        /// The <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> and <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRenderAsync(System.Boolean)" /> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender" /> parameter to ensure that initialization work is only performed
        /// once.
        /// </remarks>
        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (firstRender)
            {
                this.TreeView.SetNodeExpanded(x => x.Visible, true);
            }
        }

        /// <summary>
        /// Method that is executed when a form popup is closed
        /// </summary>
        private void OnClosedFormPopup()
        {
            this.IsFileFormVisibile = false;
            this.IsFolderFormVisibile = false;
        }

        /// <summary>
        /// Method invoked when a node is dragged
        /// </summary>
        /// <param name="node">The dragged node</param>
        private void OnDragNode(FileFolderNodeViewModel node)
        {
            this.DraggedNode = node;
        }

        /// <summary>
        /// Method invoked when a node is dropped
        /// </summary>
        /// <param name="targetNode">The target node where the <see cref="DraggedNode"/> has been dropped</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task OnDropNode(FileFolderNodeViewModel targetNode)
        {
            if (targetNode.Thing is not Folder and not null)
            {
                return;
            }

            var targetFolder = (Folder)targetNode.Thing;

            switch (this.DraggedNode.Thing)
            {
                case File file:
                    await this.ViewModel.FileHandlerViewModel.MoveFile(file, targetFolder);
                    break;
                case Folder folder:
                    await this.ViewModel.FolderHandlerViewModel.MoveFolder(folder, targetFolder);
                    break;
            }
        }
    }
}
