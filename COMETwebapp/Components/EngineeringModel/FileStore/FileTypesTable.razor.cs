// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileTypesTable.razor.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Components.EngineeringModel.FileStore
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMET.Web.Common.Components;

    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    ///  Support class for the <see cref="FileTypesTable"/>
    /// </summary>
    public partial class FileTypesTable : DisposableComponent
    {
        /// <summary>
        /// A collection of file types to display for selection
        /// </summary>
        [Parameter]
        public IEnumerable<FileType> FileTypes { get; set; }

        /// <summary>
        /// A collection of selected file types to display for selection
        /// </summary>
        [Parameter]
        public OrderedItemList<FileType> SelectedFileTypes { get; set; }

        /// <summary>
        /// The method that is executed when the selected file types change
        /// </summary>
        [Parameter]
        public EventCallback<OrderedItemList<FileType>> SelectedFileTypesChanged { get; set; }

        /// <summary>
        /// The file type that will be added
        /// </summary>
        public FileType FileType { get; private set; }

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid Grid { get; set; }

        /// <summary>
        /// Method that is invoked when the edit/add file type form is being saved
        /// </summary>
        private async Task OnEditFileTypesSaving()
        {
            var listOfFileTypes = this.SelectedFileTypes;
            listOfFileTypes.Add(this.FileType);

            this.SelectedFileTypes = listOfFileTypes;
            await this.SelectedFileTypesChanged.InvokeAsync(this.SelectedFileTypes);
        }

        /// <summary>
        /// Moves the selected row up
        /// </summary>
        /// <param name="row">The row to be moved</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task MoveUp(FileTypeRowViewModel row)
        {
            var currentIndex = this.SelectedFileTypes.IndexOf(row.Thing);
            this.SelectedFileTypes.Move(currentIndex, currentIndex - 1);
            await this.SelectedFileTypesChanged.InvokeAsync(this.SelectedFileTypes);
        }

        /// <summary>
        /// Moves the selected row down
        /// </summary>
        /// <param name="row">The row to be moved</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task MoveDown(FileTypeRowViewModel row)
        {
            var currentIndex = this.SelectedFileTypes.IndexOf(row.Thing);
            this.SelectedFileTypes.Move(currentIndex, currentIndex + 1);
            await this.SelectedFileTypesChanged.InvokeAsync(this.SelectedFileTypes);
        }

        /// <summary>
        /// Method that is invoked when a file type row is being removed
        /// </summary>
        private async Task RemoveFileType(FileTypeRowViewModel row)
        {
            this.SelectedFileTypes.Remove(row.Thing);
            await this.SelectedFileTypesChanged.InvokeAsync(this.SelectedFileTypes);
        }

        /// <summary>
        /// Method invoked when creating a new file type
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        private void CustomizeEditFileType(GridCustomizeEditModelEventArgs e)
        {
            this.FileType = new FileType();
            e.EditModel = this.FileType;
        }

        /// <summary>
        /// Method used to retrieve the available rows, given the <see cref="FileType"/>
        /// </summary>
        /// <returns>A collection of <see cref="FileTypeRowViewModel"/>s to display</returns>
        private List<FileTypeRowViewModel> GetRows()
        {
            return this.SelectedFileTypes.Select(x => new FileTypeRowViewModel(x)).ToList();
        }
    }
}
