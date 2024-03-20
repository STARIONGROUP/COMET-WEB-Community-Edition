// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMeasurementScalesTableViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using DynamicData;

    /// <summary>
    /// View model used to manage <see cref="MeasurementScale" />s
    /// </summary>
    public interface IMeasurementScalesTableViewModel : IApplicationBaseViewModel, IHaveReusableRows
    {
        /// <summary>
        /// Gets or sets the data source for the grid control.
        /// </summary>
        SourceList<MeasurementScale> DataSource { get; }

        /// <summary>
        /// A reactive collection of <see cref="MeasurementScaleRowViewModel" />
        /// </summary>
        SourceList<MeasurementScaleRowViewModel> Rows { get; }

        /// <summary>
        /// Injected property to get access to <see cref="IShowHideDeprecatedThingsService" />
        /// </summary>
        IShowHideDeprecatedThingsService ShowHideDeprecatedThingsService { get; }

        /// <summary>
        /// The <see cref="MeasurementScale" /> to create or edit
        /// </summary>
        MeasurementScale MeasurementScale { get; set; }

        /// <summary>
        /// Indicates if confirmation popup is visible
        /// </summary>
        bool IsOnDeprecationMode { get; set; }

        /// <summary>
        /// popup message dialog
        /// </summary>
        string PopupDialog { get; set; }

        /// <summary>
        /// Gets the available <see cref="ReferenceDataLibrary" />s
        /// </summary>
        IEnumerable<ReferenceDataLibrary> ReferenceDataLibraries { get; }

        /// <summary>
        /// Gets the available <see cref="MeasurementUnit" />s
        /// </summary>
        IEnumerable<MeasurementUnit> MeasurementUnits { get; }

        /// <summary>
        /// Gets the available <see cref="NumberSetKind" />s
        /// </summary>
        IEnumerable<NumberSetKind> NumberSetKinds { get; }

        /// <summary>
        /// Method invoked when confirming the deprecation/un-deprecation of a <see cref="MeasurementScalesTableViewModel.MeasurementScale" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task OnConfirmPopupButtonClick();

        /// <summary>
        /// Method invoked when canceling the deprecation/un-deprecation of a <see cref="MeasurementScalesTableViewModel.MeasurementScale" />
        /// </summary>
        void OnCancelPopupButtonClick();

        /// <summary>
        /// Action invoked when the deprecate or undeprecate button is clicked
        /// </summary>
        /// <param name="measurementScaleRow"> The <see cref="MeasurementScaleRowViewModel" /> to deprecate or undeprecate </param>
        void OnDeprecateUnDeprecateButtonClick(MeasurementScaleRowViewModel measurementScaleRow);

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// Override this method if you will perform an asynchronous operation and
        /// want the component to refresh when that operation is completed.
        /// </summary>
        void InitializeViewModel();

        /// <summary>
        /// Tries to deprecate or undeprecate a <see cref="MeasurementScalesTableViewModel.MeasurementScale" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task DeprecatingOrUnDeprecatingMeasurementScale();
    }
}
