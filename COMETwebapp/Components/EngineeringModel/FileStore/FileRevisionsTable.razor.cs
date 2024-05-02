// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileRevisionsTable.razor.cs" company="Starion Group S.A.">
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
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Components;

    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileRevisionHandler;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;

    /// <summary>
    ///  Support class for the <see cref="FileRevisionsTable"/>
    /// </summary>
    public partial class FileRevisionsTable : DisposableComponent
    {
        /// <summary>
        /// Gets or sets the <see cref="IFileRevisionHandlerViewModel"/>
        /// </summary>
        [Parameter]
        public IFileRevisionHandlerViewModel ViewModel { get; set; }

        /// <summary>
        /// A collection of file revisions to display for selection
        /// </summary>
        [Parameter]
        public IEnumerable<FileRevision> FileRevisions { get; set; }

        /// <summary>
        /// The method that is executed when the file revisions change
        /// </summary>
        [Parameter]
        public EventCallback<IEnumerable<FileRevision>> FileRevisionsChanged { get; set; }

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid Grid { get; set; }

        /// <summary>
        /// Method that is invoked when the edit/add file revision form is being saved
        /// </summary>
        private async Task OnEditFileRevisionSaving()
        {
            var listOfFileRevisions = this.FileRevisions.ToList();
            listOfFileRevisions.Add(this.ViewModel.FileRevision);

            this.FileRevisions = listOfFileRevisions;
            await this.FileRevisionsChanged.InvokeAsync(this.FileRevisions);
        }

        /// <summary>
        /// Method that is invoked when a file revision row is being removed
        /// </summary>
        private async Task RemoveFileRevision(FileRevisionRowViewModel row)
        {
            var listOfFileRevisions = this.FileRevisions.ToList();
            listOfFileRevisions.Remove(row.Thing);

            this.FileRevisions = listOfFileRevisions;
            await this.FileRevisionsChanged.InvokeAsync(this.FileRevisions);
        }

        /// <summary>
        /// Method invoked when creating a new file revision
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        private void CustomizeEditFileRevision(GridCustomizeEditModelEventArgs e)
        {
            this.ViewModel.FileRevision = new FileRevision
            {
                ContainingFolder = this.ViewModel.CurrentFile.CurrentContainingFolder
            };

            e.EditModel = this.ViewModel.FileRevision;
        }

        /// <summary>
        /// Method used to retrieve the available rows, given the <see cref="FileRevision"/> from <see cref="File"/>
        /// </summary>
        /// <returns>A collection of <see cref="FileRevisionRowViewModel"/>s to display</returns>
        private List<FileRevisionRowViewModel> GetRows()
        {
            return this.FileRevisions.Select(x => new FileRevisionRowViewModel(x)).ToList();
        }

        /// <summary>
        /// Method that is invoked when a file is uploaded to server
        /// </summary>
        private async Task OnFileUpload(InputFileChangeEventArgs e)
        {
            await this.ViewModel.UploadFile(e.File);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Value asserting if this component should dispose or not</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.ViewModel.Dispose();
        }
    }
}
