﻿// --------------------------------------------------------------------------------------------------------------------
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

namespace COMETwebapp.ViewModels.Components.ReferenceData.MeasurementUnits
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.Common.DeprecatableDataItemTable;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;
    using COMETwebapp.Wrappers;

    /// <summary>
    /// View model used to manage <see cref="MeasurementUnit" />s
    /// </summary>
    public interface IMeasurementUnitsTableViewModel : IDeprecatableDataItemTableViewModel<MeasurementUnit, MeasurementUnitRowViewModel>
    {
        /// <summary>
        /// Gets the available <see cref="ReferenceDataLibrary" />s
        /// </summary>
        IEnumerable<ReferenceDataLibrary> ReferenceDataLibraries { get; }

        /// <summary>
        /// Gets or sets the selected reference data library
        /// </summary>
        ReferenceDataLibrary SelectedReferenceDataLibrary { get; set; }

        /// <summary>
        /// Gets the available measurement unit types <see cref="ClassKind" />s
        /// </summary>
        IEnumerable<ClassKindWrapper> MeasurementUnitTypes { get; }

        /// <summary>
        /// Gets or sets the selected measurement unit type
        /// </summary>
        ClassKindWrapper SelectedMeasurementUnitType { get; set; }

        /// <summary>
        /// Gets the available <see cref="MeasurementUnit" />s from the same rdl as the <see cref="SelectedReferenceDataLibrary"/>
        /// </summary>
        IEnumerable<MeasurementUnit> ReferenceUnits { get; }

        /// <summary>
        /// Gets the available <see cref="Prefixes" />s from the same rdl as the <see cref="SelectedReferenceDataLibrary"/>
        /// </summary>
        IEnumerable<UnitPrefix> Prefixes { get; }

        /// <summary>
        /// Creates or edits a <see cref="MeasurementUnit"/>
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="MeasurementUnit"/> should be created</param>
        /// <returns>A <see cref="Task"/></returns>
        Task CreateOrEditMeasurementUnit(bool shouldCreate);
    }
}
