﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FolderFileStructure.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.Components.EngineeringModel
{
    using System.ComponentModel.DataAnnotations;

    using CDP4Common.EngineeringModelData;

    using COMETwebapp.ViewModels.Components.EngineeringModel.FolderFileStructure;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

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
        /// Gets the selected file from the <see cref="TreeView"/>
        /// </summary>
        public File SelectedFile { get; private set; }

        /// <summary>
        /// Gets the selected folder from the <see cref="TreeView"/>
        /// </summary>
        public Folder SelectedFolder { get; private set; }

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

            if (dataItem.Thing is not File file)
            {
                return;
            }

            this.ViewModel.File = file;
            this.SelectedFile = this.ViewModel.File;
        }

        /// <summary>
        /// Method that is triggered every time the edit folder icon is clicked
        /// </summary>
        /// <param name="row">The selected row</param>
        public void OnEditFolderClick(FileFolderNodeViewModel row)
        {
            if (row.Thing is not Folder folder)
            {
                return;
            }

            this.ViewModel.Folder = folder;
            this.SelectedFolder = this.ViewModel.Folder;
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
            this.SelectedFile = null;
            this.SelectedFolder = null;
        }
    }
}