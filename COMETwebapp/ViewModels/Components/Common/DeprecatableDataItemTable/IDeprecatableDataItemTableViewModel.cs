// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDeprecatableDataItemTableViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.Common.DeprecatableDataItemTable
{
    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.Common.BaseDataItemTable;

    using DynamicData;

    /// <summary>
    /// View model that provides the basic functionalities for a reference data item
    /// </summary>
    public interface IDeprecatableDataItemTableViewModel<T, TRow> : IBaseDataItemTableViewModel<T, TRow>
    {
        /// <summary>
        /// Injected property to get access to <see cref="IShowHideDeprecatedThingsService" />
        /// </summary>
        IShowHideDeprecatedThingsService ShowHideDeprecatedThingsService { get; }

        /// <summary>
        /// Indicates if confirmation popup is visible
        /// </summary>
        bool IsOnDeprecationMode { get; set; }

        /// <summary>
        /// Gets or sets the popup message dialog
        /// </summary>
        string PopupDialog { get; set; }

        /// <summary>
        /// Method invoked when confirming the deprecation/un-deprecation of the <see cref="DeprecatableDataItemTableViewModel{T,TRow}"/>
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task OnConfirmPopupButtonClick();

        /// <summary>
        /// Method invoked when canceling the deprecation/un-deprecation of the <see cref="DeprecatableDataItemTableViewModel{T,TRow}"/>
        /// </summary>
        void OnCancelPopupButtonClick();

        /// <summary>
        /// Action invoked when the deprecate or undeprecate button is clicked
        /// </summary>
        /// <param name="thingRow"> The thing to deprecate or undeprecate </param>
        void OnDeprecateUnDeprecateButtonClick(TRow thingRow);

        /// <summary>
        /// Tries to deprecate or undeprecate the <see cref="DeprecatableDataItemTableViewModel{T,TRow}"/>
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task DeprecateOrUnDeprecateThing();
    }
}
