// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDeletableDataItemTableViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.Common.DeletableDataItemTable
{
    using COMETwebapp.ViewModels.Components.Common.BaseDataItemTable;

    /// <summary>
    /// View model that provides the basic functionalities for a reference data item
    /// </summary>
    public interface IDeletableDataItemTableViewModel<T, TRow> : IBaseDataItemTableViewModel<T, TRow>
    {
        /// <summary>
        /// Indicates if confirmation popup is visible
        /// </summary>
        bool IsOnDeletionMode { get; set; }

        /// <summary>
        /// Gets or sets the popup message dialog
        /// </summary>
        string PopupDialog { get; set; }

        /// <summary>
        /// Method invoked when confirming the deletion of the <see cref="DeletableDataItemTableViewModel{T,TRow}"/>
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task OnConfirmPopupButtonClick();

        /// <summary>
        /// Method invoked when canceling the deletion of the <see cref="DeletableDataItemTableViewModel{T,TRow}"/>
        /// </summary>
        void OnCancelPopupButtonClick();

        /// <summary>
        /// Action invoked when the delete button is clicked
        /// </summary>
        /// <param name="thingRow"> The row to delete </param>
        void OnDeleteButtonClick(TRow thingRow);

        /// <summary>
        /// Tries to delete the current thing
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task DeleteThing();
    }
}
