// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMeasurementUnitsTableViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.ReferenceData
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using DynamicData;

    /// <summary>
    /// View model used to manage <see cref="MeasurementUnit" />s
    /// </summary>
    public interface IReferenceDataItemViewModel<T> : IApplicationBaseViewModel, IHaveReusableRows where T : DefinedThing, IDeprecatableThing
    {
        /// <summary>
        /// Injected property to get access to <see cref="IShowHideDeprecatedThingsService" />
        /// </summary>
        IShowHideDeprecatedThingsService ShowHideDeprecatedThingsService { get; }

        /// <summary>
        /// Gets or sets the data source for the grid control.
        /// </summary>
        SourceList<T> DataSource { get; }

        /// <summary>
        /// Indicates if confirmation popup is visible
        /// </summary>
        bool IsOnDeprecationMode { get; set; }

        /// <summary>
        /// The <see cref="MeasurementUnit" /> to create or edit
        /// </summary>
        T MeasurementUnit { get; set; }

        /// <summary>
        /// popup message dialog
        /// </summary>
        string PopupDialog { get; set; }

        /// <summary>
        /// Initializes the <see cref="MeasurementUnitsTableViewModel"/>
        /// </summary>
        void InitializeViewModel();

        /// <summary>
        /// Method invoked when confirming the deprecation/un-deprecation of a <see cref="ReferenceDataItemViewModel{T}.MeasurementUnit" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task OnConfirmPopupButtonClick();

        /// <summary>
        /// Method invoked when canceling the deprecation/un-deprecation of a <see cref="ReferenceDataItemViewModel{T}.MeasurementUnit" />
        /// </summary>
        void OnCancelPopupButtonClick();

        /// <summary>
        /// Action invoked when the deprecate or undeprecate button is clicked
        /// </summary>
        /// <param name="measurementUnitRow"> The <see cref="MeasurementUnitRowViewModel" /> to deprecate or undeprecate </param>
        void OnDeprecateUnDeprecateButtonClick(ReferenceDataItemRowViewModel<T> measurementUnitRow);

        /// <summary>
        /// Tries to deprecate or undeprecate a <see cref="ReferenceDataItemViewModel{T}.MeasurementUnit" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task DeprecatingOrUnDeprecatingMeasurementUnit();

        /// <summary>
        /// Add rows related to <see cref="Thing" /> that has been added
        /// </summary>
        /// <param name="addedThings">A collection of added <see cref="Thing" /></param>
        void AddRows(IEnumerable<Thing> addedThings);

        /// <summary>
        /// Updates rows related to <see cref="Thing" /> that have been updated
        /// </summary>
        /// <param name="updatedThings">A collection of updated <see cref="Thing" /></param>
        void UpdateRows(IEnumerable<Thing> updatedThings);

        /// <summary>
        /// Remove rows related to a <see cref="Thing" /> that has been deleted
        /// </summary>
        /// <param name="deletedThings">A collection of deleted <see cref="Thing" /></param>
        void RemoveRows(IEnumerable<Thing> deletedThings);
    }
}
