// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FileRevisionsTable.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.EngineeringModel.FileStore
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileRevisionHandler;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="FileRevisionsTable" />
    /// </summary>
    public partial class FileRevisionsTable : DisposableComponent
    {
        /// <summary>
        /// Gets or sets a value indicating whether the active user may write the parent thing this table edits part of.
        /// The rows here are parts of one aggregate saved atomically by the hosting form, so the permission is decided
        /// once by that form and passed down rather than evaluated per row. Defaults to true so a host that does not
        /// set it keeps its previous behaviour
        /// </summary>
        [Parameter]
        public bool IsAllowedToWrite { get; set; } = true;

        /// <summary>
        /// The injected <see cref="ISessionService" />, used to assert whether the open session allows writing
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets a value indicating whether the open session forbids any modification, which is the case for a session
        /// opened from an ECSS-E-TM-10-25 Annex C3 archive. Create and delete controls bind their enabled state to
        /// the inverse of this, so the data can still be inspected but never modified
        /// </summary>
        public bool IsReadOnly => this.SessionService.IsReadOnly;

        /// <summary>
        /// Gets or sets the <see cref="IFileRevisionHandlerViewModel" />
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
        /// Gets or sets a value indicating whether the form popup is visible for editing or creating.
        /// </summary>
        public bool IsOnEditMode { get; set; }

        /// <summary>
        /// Starts the creation flow for a new <see cref="FileRevision" /> item.
        /// </summary>
        public void StartCreate()
        {
            this.ViewModel.FileRevision = new FileRevision
            {
                ContainingFolder = this.ViewModel.CurrentFile.CurrentContainingFolder
            };

            this.IsOnEditMode = true;
        }

        /// <summary>
        /// Method that is invoked when the edit/add file revision form is being saved
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task OnSaved()
        {
            var listOfFileRevisions = this.FileRevisions.ToList();
            listOfFileRevisions.Add(this.ViewModel.FileRevision);

            this.FileRevisions = listOfFileRevisions;
            await this.FileRevisionsChanged.InvokeAsync(this.FileRevisions);

            this.IsOnEditMode = false;
        }

        /// <summary>
        /// Handles cancellation of the form popup.
        /// </summary>
        public void OnCanceled()
        {
            this.IsOnEditMode = false;
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

        /// <summary>
        /// Method that is invoked when a file revision row is being removed
        /// </summary>
        /// <param name="row">The selected row to remove</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task RemoveFileRevision(FileRevisionRowViewModel row)
        {
            var listOfFileRevisions = this.FileRevisions.ToList();
            listOfFileRevisions.Remove(row.Thing);

            this.FileRevisions = listOfFileRevisions;
            await this.FileRevisionsChanged.InvokeAsync(this.FileRevisions);
        }

        /// <summary>
        /// Method used to retrieve the available rows, given the <see cref="FileRevision" /> from <see cref="File" />
        /// </summary>
        /// <returns>A collection of <see cref="FileRevisionRowViewModel" />s to display</returns>
        private List<FileRevisionRowViewModel> GetRows()
        {
            return this.FileRevisions.Select(x => new FileRevisionRowViewModel(x)).ToList();
        }
    }
}
